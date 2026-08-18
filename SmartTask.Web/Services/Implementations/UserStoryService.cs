using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class UserStoryService : BaseService<UserStory>, IUserStoryService
    {
        private readonly ApplicationDbContext _context;

        public UserStoryService(
            IGenericRepository<UserStory> repository,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
            : base(repository, unitOfWork)
        {
            _context = context;
        }

        public async Task<UserStory?> GetDetailsAsync(int id)
        {
            return await _context.UserStories
                .Include(x => x.Project)
                .Include(x => x.Backlog)
                .Include(x => x.Sprint)
                .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
        }

        public async Task<List<UserStory>> GetBacklogStoriesAsync(int projectId)
        {
            return await _context.UserStories
                .Where(x => x.ProjectId == projectId && x.SprintId == null && x.ViewState)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<List<UserStory>> GetSprintStoriesAsync(int sprintId)
        {
            return await _context.UserStories
                .Where(x => x.SprintId == sprintId && x.ViewState)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }

        public async Task<bool> ExistsByTitleAsync(
            int backlogId,
            string title,
            int? excludeId = null)
        {
            var query = _repository.Query()
                .Where(x => x.BacklogId == backlogId && x.Title == title && x.ViewState);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanManageBacklogAsync(int projectId, int userId)
        {
            return await _context.Projects
                .Where(p => p.Id == projectId)
                .AnyAsync(p =>
                    p.Workspace.OwnerId == userId ||
                    p.Workspace.Members.Any(m =>
                        m.ApplicationUserId == userId &&
                        m.ViewState &&
                        (m.Role == WorkspaceRoleType.Owner || m.Role == WorkspaceRoleType.Admin)));
        }

        public async Task<bool> CanManageStoryAsync(int storyId, int userId)
        {
            var projectId = await _repository.Query()
                .Where(x => x.Id == storyId)
                .Select(x => x.ProjectId)
                .FirstOrDefaultAsync();

            if (projectId == 0)
                return false;

            return await CanManageBacklogAsync(projectId, userId);
        }

        public async Task MoveToSprintAsync(int storyId, int sprintId)
        {
            await _context.UserStories
                .Where(x => x.Id == storyId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.SprintId, sprintId)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        public async Task RemoveFromSprintAsync(int storyId)
        {
            var story = await _context.UserStories
                .Select(x => new { x.Id, x.ProjectId, x.SprintId })
                .FirstOrDefaultAsync(x => x.Id == storyId);

            if (story == null)
                return;

            var maxOrder = await _context.UserStories
                .Where(x => x.ProjectId == story.ProjectId && x.SprintId == null && x.ViewState)
                .MaxAsync(x => (int?)x.Order) ?? -1;

            await _context.UserStories
                .Where(x => x.Id == storyId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.SprintId, (int?)null)
                    .SetProperty(x => x.Order, maxOrder + 1)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        public async Task ChangePriorityAsync(int storyId, StoryPriorityType priority)
        {
            await _context.UserStories
                .Where(x => x.Id == storyId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Priority, priority)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        public async Task ChangeStatusAsync(int storyId, StoryStatusType status)
        {
            await _context.UserStories
                .Where(x => x.Id == storyId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, status)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        public async Task ReorderAsync(List<int> orderedIds)
        {
            var now = DateTime.Now;
            var stories = await _context.UserStories
                .Where(x => orderedIds.Contains(x.Id) && x.ViewState)
                .ToListAsync();

            // Batch update using direct updates
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var id = orderedIds[i];
                await _context.UserStories
                    .Where(x => x.Id == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Order, i)
                        .SetProperty(x => x.ChangeDate, now));
            }
        }

        public new async Task DeleteAsync(int id)
        {
            await _context.UserStories
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ViewState, false));
        }

        public async Task ChangeOwnerAsync(int storyId, int? ownerId)
        {
            await _context.UserStories
                .Where(x => x.Id == storyId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.OwnerId, ownerId)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }

        public async Task<Dictionary<int, List<string>>> GetContributorsMapAsync(int projectId)
        {
            return await _context.TaskAssignments
                .Where(x =>
                    x.ViewState &&
                    x.TaskItem.ViewState &&
                    x.TaskItem.UserStory.ProjectId == projectId)
                .Select(x => new
                {
                    x.TaskItem.UserStoryId,
                    x.ApplicationUser.FullName
                })
                .GroupBy(x => x.UserStoryId)
                .Select(g => new
                {
                    StoryId = g.Key,
                    Contributors = g.Select(x => x.FullName).Distinct().ToList()
                })
                .ToDictionaryAsync(x => x.StoryId, x => x.Contributors);
        }
    }
}
