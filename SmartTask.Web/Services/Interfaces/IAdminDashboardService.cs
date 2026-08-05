using SmartTask.Web.Models.ViewModels.Admin;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
    }
}