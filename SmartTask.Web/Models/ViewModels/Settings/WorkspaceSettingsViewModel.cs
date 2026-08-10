using Microsoft.AspNetCore.Mvc.Rendering;
namespace SmartTask.Web.Models.ViewModels.Settings
{
    public class WorkspaceSettingsViewModel
    {
        public int? DefaultWorkspaceId { get; set; }
        public List<SelectListItem> Workspaces { get; set; } = new();
    }
}