using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Workspace;
namespace SmartTask.Web.Services.Interfaces
{
    public interface IUserService : IBaseService<ApplicationUser>
    {
        Task<List<UserSearchResultViewModel>> SearchUsersAsync(
            string term,
            int workspaceId);
    }
}