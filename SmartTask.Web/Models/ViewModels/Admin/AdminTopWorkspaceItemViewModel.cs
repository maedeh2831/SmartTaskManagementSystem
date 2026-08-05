namespace SmartTask.Web.Models.ViewModels.Admin
{
    public class AdminTopWorkspaceItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = "#4F46E5";
        public int ProjectsCount { get; set; }
        public int MembersCount { get; set; }
    }
}