namespace SmartTask.Web.Models.ViewModels.Search
{
    public class QuickAddProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Key { get; set; } = "";
        public string WorkspaceName { get; set; } = "";
        public string Color { get; set; } = "#4F46E5";
        public string Icon { get; set; } = "fa-solid fa-diagram-project";
    }
}