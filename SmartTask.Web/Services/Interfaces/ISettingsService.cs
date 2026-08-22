using SmartTask.Web.Models.ViewModels.Settings;

namespace SmartTask.Web.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<SettingsViewModel> GetSettingsAsync(int userId);
        Task UpdateAccountAsync(int userId, AccountSettingsViewModel model);
        Task UpdateAppearanceAsync(int userId, AppearanceSettingsViewModel model);
        Task UpdateNotificationsAsync(int userId, List<NotificationPreferenceItemViewModel> model);
        Task UpdateDefaultWorkspaceAsync(int userId, int? workspaceId);
        Task UpdateManagementAsync(int userId, ManagementSettingsViewModel model);
    }
}