using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Navigation
{
    public class SidebarItem
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = "Index";
        public string? Badge { get; set; }
        public string Category { get; set; } = "";
        public string? RequiredRole { get; set; }
        public ContextRequirementType RequiresContext { get; set; } = ContextRequirementType.None; // 👈 جدید
    }
}