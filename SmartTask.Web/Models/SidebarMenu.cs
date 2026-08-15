using SmartTask.Web.Models.Enums;
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
                Controller="Home",
                RequiresContext = ContextRequirementType.None
            },
            new()
            {
                Title="فضاهای کاری",
                Category="مدیریت",
                Icon="fa-solid fa-layer-group",
                Controller="Workspace",
                RequiresContext = ContextRequirementType.None
            },
            new()
            {
                Title="پروژه‌ها",
                Category="مدیریت",
                Icon="fa-solid fa-folder-open",
                Controller="Project",
                RequiresContext = ContextRequirementType.Workspace
            },
            new()
            {
                Title="تیم‌ها",
                Category="مدیریت",
                Icon="fa-solid fa-users",
                Controller="Team",
                RequiresContext = ContextRequirementType.Workspace
            },
            new()
            {
                Title="وظایف",
                Category="مدیریت",
                Icon="fa-solid fa-square-check",
                Controller="TaskBoard", // 👈 تغییر از Task به TaskBoard
                RequiresContext = ContextRequirementType.Project
            },
            new()
            {
                Title="بک‌لاگ",
                Category="برنامه‌ریزی",
                Icon="fa-solid fa-list-check",
                Controller="Backlog",
                RequiresContext = ContextRequirementType.Project
            },
            new()
            {
                Title="اسپرینت‌ها",
                Category="برنامه‌ریزی",
                Icon="fa-solid fa-bolt",
                Controller="Sprint",
                RequiresContext = ContextRequirementType.Project
            },
            new()
            {
                Title="گفتگو",
                Category="مدیریت",
                Icon="fa-solid fa-comments",
                Controller="Chat",
                RequiresContext = ContextRequirementType.None
            },
            new()
            {
                Title="گزارش‌ها",
                Category="تحلیل",
                Icon="fa-solid fa-chart-line",
                Controller="Report",
                RequiresContext = ContextRequirementType.None
            },
            new()
            {
                Title="داشبورد مدیریت",
                Category="سیستم",
                Icon="fa-solid fa-shield-halved",
                Controller="Admin",
                RequiredRole="Admin",
                RequiresContext = ContextRequirementType.None
            }
        };
    }
}