using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ICurrentUserService CurrentUser;

        protected BaseController(ICurrentUserService currentUser)
        {
            CurrentUser = currentUser;
        }

        /// <summary>
        /// Check if the current user is an active member of the given project.
        /// </summary>
        protected async Task<bool> IsProjectMemberAsync(ApplicationDbContext context, int projectId)
        {
            return await context.ProjectMembers
                .AnyAsync(x => x.ProjectId == projectId
                    && x.ApplicationUserId == CurrentUser.UserId
                    && x.ViewState);
        }

        /// <summary>
        /// Check if the current user is an active member of the given workspace.
        /// </summary>
        protected async Task<bool> IsWorkspaceMemberAsync(ApplicationDbContext context, int workspaceId)
        {
            return await context.WorkspaceMembers
                .AnyAsync(x => x.WorkspaceId == workspaceId
                    && x.ApplicationUserId == CurrentUser.UserId
                    && x.ViewState);
        }

        /// <summary>
        /// Get the workspace ID that a project belongs to.
        /// </summary>
        protected async Task<int?> GetProjectWorkspaceIdAsync(ApplicationDbContext context, int projectId)
        {
            return await context.Projects
                .Where(x => x.Id == projectId)
                .Select(x => (int?)x.WorkspaceId)
                .FirstOrDefaultAsync();
        }
    }
}