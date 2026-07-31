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
            var query = _repository
                .Query()
                .Where(x => x.BacklogId == backlogId && x.Title == title && x.ViewState);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanManageBacklogAsync(int projectId, int userId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId);

            if (project == null)
                return false;

            var isWorkspaceOwner = await _context.Workspaces
                .AnyAsync(x => x.Id == project.WorkspaceId && x.OwnerId == userId);

            if (isWorkspaceOwner)
                return true;

            return await _context.WorkspaceMembers
                .AnyAsync(x =>
                    x.WorkspaceId == project.WorkspaceId &&
                    x.ApplicationUserId == userId &&
                    x.ViewState &&
                    (x.Role == WorkspaceRoleType.Owner || x.Role == WorkspaceRoleType.Admin));
        }

        public async Task<bool> CanManageStoryAsync(int storyId, int userId)
        {
            var story = await _repository
                .Query()
                .FirstOrDefaultAsync(x => x.Id == storyId);

            if (story == null)
                return false;

            return await CanManageBacklogAsync(story.ProjectId, userId);
        }

        public async Task MoveToSprintAsync(int storyId, int sprintId)
        {
            var story = await _context.UserStories
                .FirstOrDefaultAsync(x => x.Id == storyId);

            if (story == null)
                return;

            story.SprintId = sprintId;
            story.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromSprintAsync(int storyId)
        {
            var story = await _context.UserStories
                .FirstOrDefaultAsync(x => x.Id == storyId);

            if (story == null)
                return;

            var maxOrder = await _context.UserStories
                .Where(x => x.ProjectId == story.ProjectId && x.SprintId == null && x.ViewState)
                .Select(x => (int?)x.Order)
                .MaxAsync() ?? -1;

            story.SprintId = null;
            story.Order = maxOrder + 1;
            story.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task ChangePriorityAsync(int storyId, StoryPriorityType priority)
        {
            var story = await _context.UserStories
                .FirstOrDefaultAsync(x => x.Id == storyId);

            if (story == null)
                return;

            story.Priority = priority;
            story.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task ChangeStatusAsync(int storyId, StoryStatusType status)
        {
            var story = await _context.UserStories
                .FirstOrDefaultAsync(x => x.Id == storyId);

            if (story == null)
                return;

            story.Status = status;
            story.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task ReorderAsync(List<int> orderedIds)
        {
            var stories = await _context.UserStories
                .Where(x => orderedIds.Contains(x.Id) && x.ViewState)
                .ToListAsync();

            for (int i = 0; i < orderedIds.Count; i++)
            {
                var story = stories.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (story != null)
                {
                    story.Order = i;
                    story.ChangeDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }

        public new async Task DeleteAsync(int id)
        {
            var story = await _context.UserStories
                .FirstOrDefaultAsync(x => x.Id == id);

            if (story == null)
                return;

            story.ViewState = false;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeOwnerAsync(int storyId, int? ownerId)
        {
            var story = await _context.UserStories
                .FirstOrDefaultAsync(x => x.Id == storyId);

            if (story == null)
                return;

            story.OwnerId = ownerId;
            story.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<Dictionary<int, List<string>>> GetContributorsMapAsync(int projectId)
        {
            var data = await _context.TaskAssignments
                .Where(x =>
                    x.ViewState &&
                    x.TaskItem.ViewState &&
                    x.TaskItem.UserStory.ProjectId == projectId)
                .Select(x => new
                {
                    x.TaskItem.UserStoryId,
                    x.ApplicationUser.FullName
                })
                .Distinct()
                .ToListAsync();

            return data
                .GroupBy(x => x.UserStoryId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.FullName).ToList());
        }
    }
}