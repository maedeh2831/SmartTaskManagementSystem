using SmartTask.Web.Models.ViewModels.Home;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IUserDashboardService
    {
        Task<UserDashboardViewModel> GetDashboardAsync(int userId);
    }
}