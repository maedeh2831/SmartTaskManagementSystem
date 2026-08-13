using SmartTask.Web.Models.ViewModels.Workspace;

namespace SmartTask.Web.Models.ViewModels.Settings
{
    public class SettingsViewModel
    {
        public AccountSettingsViewModel Account { get; set; } = new();
        public List<NotificationPreferenceItemViewModel> Notifications { get; set; } = new();
        public AppearanceSettingsViewModel Appearance { get; set; } = new();
        public WorkspaceSettingsViewModel Workspace { get; set; } = new();
    }
}