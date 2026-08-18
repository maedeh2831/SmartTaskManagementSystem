using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.ViewModels.Settings
{
    public class AppearanceSettingsViewModel
    {
        public ThemeType Theme { get; set; }
        public TaskDensityType TaskDensity { get; set; }
    }
}