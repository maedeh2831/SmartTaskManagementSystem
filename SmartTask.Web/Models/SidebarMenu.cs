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
                Category="اصلی",
                Icon="fa-solid fa-house",
                Controller="Home"
            },

            new()
            {
                Title="فضاهای کاری",
                Category="مدیریت",
                Icon="fa-solid fa-layer-group",
                Controller="Workspace"
            },

            new()
            {
                Title="پروژه‌ها",
                Category="مدیریت",
                Icon="fa-solid fa-folder-open",
                Controller="Project",
                Badge="12"
            },

            new()
            {
                Title="تیم‌ها",
                Category="مدیریت",
                Icon="fa-solid fa-users",
                Controller="Team"
            },

            new()
            {
                Title="وظایف",
                Category="مدیریت",
                Icon="fa-solid fa-square-check",
                Controller="Task",
                Badge="25"
            },

            new()
            {
                Title="بک‌لاگ",
                Category="برنامه‌ریزی",
                Icon="fa-solid fa-list-check",
                Controller="Backlog"
            },

            new()
            {
                Title="اسپرینت‌ها",
                Category="برنامه‌ریزی",
                Icon="fa-solid fa-bolt",
                Controller="Sprint",
                Badge="3"
            },

            new()
            {
                Title="گزارش‌ها",
                Category="تحلیل",
                Icon="fa-solid fa-chart-line",
                Controller="Report"
            }
        };
    }
}