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
            var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null) return;

            story.SprintId = sprintId;
            story.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromSprintAsync(int storyId)
        {
            var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null) return;

            var maxOrder = await _context.UserStories
                .Where(x => x.ProjectId == story.ProjectId && x.SprintId == null && x.ViewState)
                .MaxAsync(x => (int?)x.Order) ?? -1;

            story.SprintId = null;
            story.Order = maxOrder + 1;
            story.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task ChangePriorityAsync(int storyId, StoryPriorityType priority)
        {
            var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null) return;

            story.Priority = priority;
            story.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeStatusAsync(int storyId, StoryStatusType status)
        {
            var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null) return;

            story.Status = status;
            story.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task ReorderAsync(List<int> orderedIds)
        {
            var now = DateTime.Now;
            var stories = await _context.UserStories
                .Where(x => orderedIds.Contains(x.Id) && x.ViewState)
                .ToListAsync();

            for (int i = 0; i < orderedIds.Count; i++)
            {
                var story = stories.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (story != null)
                {
                    story.Order = i;
                    story.ChangeDate = now;
                }
            }
            await _context.SaveChangesAsync();
        }

        public new async Task DeleteAsync(int id)
        {
            var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == id);
            if (story == null) return;

            story.ViewState = false;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeOwnerAsync(int storyId, int? ownerId)
        {
            var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null) return;

            story.OwnerId = ownerId;
            story.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();
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
