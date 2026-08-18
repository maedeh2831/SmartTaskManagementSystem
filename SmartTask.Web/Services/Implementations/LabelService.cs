using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class LabelService : ILabelService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserStoryService _userStoryService;

        public LabelService(ApplicationDbContext context, IUserStoryService userStoryService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userStoryService = userStoryService ?? throw new ArgumentNullException(nameof(userStoryService));
        }

        public async Task<List<Label>> GetByProjectAsync(int projectId)
        {
            if (projectId <= 0)
                return new List<Label>();

            return await _context.Labels
                .Where(x => x.ProjectId == projectId && x.ViewState)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        /// <summary>
        /// OPTIMIZED: Get labels for multiple projects in a single query
        /// </summary>
        public async Task<Dictionary<int, List<Label>>> GetByProjectsAsync(List<int> projectIds)
        {
            if (projectIds == null || projectIds.Count == 0)
                return new Dictionary<int, List<Label>>();

            var validIds = projectIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, List<Label>>();

            var labels = await _context.Labels
                .Where(x => validIds.Contains(x.ProjectId) && x.ViewState)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return labels
                .GroupBy(x => x.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<bool> ExistsByNameAsync(int projectId, string name, int? excludeId = null)
        {
            if (projectId <= 0 || string.IsNullOrWhiteSpace(name))
                return false;

            var query = _context.Labels
                .Where(x => x.ProjectId == projectId && x.Name == name && x.ViewState);

            if (excludeId.HasValue && excludeId.Value > 0)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanManageLabelsAsync(int projectId, int userId)
        {
            if (projectId <= 0 || userId <= 0)
                return false;

            return await _userStoryService.CanManageBacklogAsync(projectId, userId);
        }

        /// <summary>
        /// OPTIMIZED: Single query for checking existence and reactivation
        /// </summary>
        public async Task CreateOrReactivateAsync(int projectId, string name, string color)
        {
            if (projectId <= 0 || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(color))
                return;

            var trimmedName = name.Trim();
            var trimmedColor = color.Trim();
            var now = DateTime.Now;

            // OPTIMIZED: Single query to check if label exists
            var existing = await _context.Labels
                .Where(x => x.ProjectId == projectId && x.Name == trimmedName)
                .Select(x => new { x.Id, x.ViewState })
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                // Create new label
                var label = new Label
                {
                    ProjectId = projectId,
                    Name = trimmedName,
                    Color = trimmedColor,
                    CreatedDate = now,
                    ViewState = true
                };

                await _context.Labels.AddAsync(label);
                await _context.SaveChangesAsync();
            }
            else if (!existing.ViewState)
            {
                // OPTIMIZED: Use ExecuteUpdateAsync to reactivate instead of load-modify-save
                await _context.Labels
                    .Where(x => x.Id == existing.Id)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.ViewState, true)
                        .SetProperty(x => x.Color, trimmedColor)
                        .SetProperty(x => x.ChangeDate, now));
            }
            // If label already exists and is active, do nothing
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.Labels
                .Where(x => x.Id == id && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch delete labels
        /// </summary>
        public async Task BatchDeleteAsync(List<int> labelIds)
        {
            if (labelIds == null || labelIds.Count == 0)
                return;

            var validIds = labelIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all deletions
            await _context.Labels
                .Where(x => validIds.Contains(x.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch create or reactivate labels
        /// </summary>
        public async Task BatchCreateOrReactivateAsync(int projectId, List<(string name, string color)> labels)
        {
            if (projectId <= 0 || labels == null || labels.Count == 0)
                return;

            var validLabels = labels
                .Where(x => !string.IsNullOrWhiteSpace(x.name) && !string.IsNullOrWhiteSpace(x.color))
                .Select(x => (name: x.name.Trim(), color: x.color.Trim()))
                .DistinctBy(x => x.name)
                .ToList();

            if (validLabels.Count == 0)
                return;

            // OPTIMIZED: Single query to get all existing labels for this project
            var existingLabels = await _context.Labels
                .Where(x => x.ProjectId == projectId && validLabels.Select(l => l.name).Contains(x.Name))
                .Select(x => new { x.Id, x.Name, x.ViewState })
                .ToListAsync();

            var existingDict = existingLabels.ToDictionary(x => x.Name);
            var now = DateTime.Now;
            var toAdd = new List<Label>();
            var toReactivate = new List<int>();

            foreach (var label in validLabels)
            {
                if (existingDict.TryGetValue(label.name, out var existing))
                {
                    if (!existing.ViewState)
                        toReactivate.Add(existing.Id);
                }
                else
                {
                    toAdd.Add(new Label
                    {
                        ProjectId = projectId,
                        Name = label.name,
                        Color = label.color,
                        CreatedDate = now,
                        ViewState = true
                    });
                }
            }

            // OPTIMIZED: Single SaveChangesAsync for all adds
            if (toAdd.Count > 0)
                await _context.Labels.AddRangeAsync(toAdd);

            // OPTIMIZED: Single ExecuteUpdateAsync for all reactivations
            if (toReactivate.Count > 0)
            {
                await _context.Labels
                    .Where(x => toReactivate.Contains(x.Id))
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.ViewState, true)
                        .SetProperty(x => x.ChangeDate, now));
            }

            if (toAdd.Count > 0)
                await _context.SaveChangesAsync();
        }

        /// <summary>
        /// OPTIMIZED: Get label usage count across tasks
        /// </summary>
        public async Task<Dictionary<int, int>> GetLabelUsageCountAsync(List<int> labelIds)
        {
            if (labelIds == null || labelIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = labelIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.TaskLabels
                .Where(x => validIds.Contains(x.LabelId) && x.ViewState)
                .GroupBy(x => x.LabelId)
                .Select(g => new { LabelId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.LabelId, x => x.Count);
        }

        /// <summary>
        /// OPTIMIZED: Update label color
        /// </summary>
        public async Task UpdateColorAsync(int labelId, string color)
        {
            if (labelId <= 0 || string.IsNullOrWhiteSpace(color))
                return;

            var trimmedColor = color.Trim();
            var now = DateTime.Now;

            // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
            await _context.Labels
                .Where(x => x.Id == labelId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.Color, trimmedColor)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch update label colors
        /// </summary>
        public async Task BatchUpdateColorsAsync(List<(int labelId, string color)> updates)
        {
            if (updates == null || updates.Count == 0)
                return;

            var validUpdates = updates
                .Where(x => x.labelId > 0 && !string.IsNullOrWhiteSpace(x.color))
                .ToList();

            if (validUpdates.Count == 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Execute updates in parallel
            var tasks = validUpdates.Select(update =>
                _context.Labels
                    .Where(x => x.Id == update.labelId && x.ViewState)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(x => x.Color, update.color.Trim())
                        .SetProperty(x => x.ChangeDate, now)))
                .ToList();

            await Task.WhenAll(tasks);
        }
    }
}
