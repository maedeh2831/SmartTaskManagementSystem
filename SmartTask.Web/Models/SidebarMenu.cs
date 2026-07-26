using SmartTask.Web.Models.Navigation;

namespace SmartTask.Web.Models
{
    public static class SidebarMenu
    {
        public static List<SidebarItem> Items => new()
        {
            new()
            {
                Title="داشبورد",
                Icon="fa-solid fa-house",
                Controller="Home"
            },

            new()
            {
                Title="Workspace",
                Icon="fa-solid fa-layer-group",
                Controller="Workspace"
            },

            new()
            {
                Title="Projects",
                Icon="fa-solid fa-folder-open",
                Controller="Project",
                Badge="12"
            },

            new()
            {
                Title="Backlog",
                Icon="fa-solid fa-list-check",
                Controller="Backlog"
            },

            new()
            {
                Title="Sprint",
                Icon="fa-solid fa-bolt",
                Controller="Sprint",
                Badge="3"
            },

            new()
            {
                Title="Tasks",
                Icon="fa-solid fa-square-check",
                Controller="Task",
                Badge="25"
            },

            new()
            {
                Title="Team",
                Icon="fa-solid fa-users",
                Controller="Team"
            },

            new()
            {
                Title="Reports",
                Icon="fa-solid fa-chart-line",
                Controller="Report"
            }
        };
    }
}