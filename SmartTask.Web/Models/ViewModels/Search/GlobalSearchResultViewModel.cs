namespace SmartTask.Web.Models.ViewModels.Search
{
    public class GlobalSearchResultViewModel
    {
        public string Type { get; set; } = "";       // "Project" | "UserStory" | "Task"
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? SubTitle { get; set; }
        public string Icon { get; set; } = "fa-solid fa-circle";
        public string Color { get; set; } = "#4F46E5";
        public string Url { get; set; } = "";
    }
}