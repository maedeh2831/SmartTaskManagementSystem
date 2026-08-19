using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.Navigation;

namespace SmartTask.Web.Models
{
    public static class SidebarMenu
    {
        public static List<SidebarItem> Items => new()
        {
            // ===== اصلی =====
            new()
            {
                Title = "داشبورد",
                Category = "اصلی",
                Icon = "fa-solid fa-house",
                Controller = "Home",
                Action = "Index",
                RequiresContext = ContextRequirementType.None
            },
            new()
            {
                Title = "یادآوری‌های من",
                Category = "اصلی",
                Icon = "fa-regular fa-clock",
                Controller = "Reminder",
                Action = "Index",
                RequiresContext = ContextRequirementType.None
            },
            new()
            {
                Title = "فعالیت‌های من",
                Category = "اصلی",
                Icon = "fa-solid fa-timeline",
                Controller = "Activity",
                Action = "Index",
                RequiresContext = ContextRequirementType.None
            },

            // ===== فضای کاری =====
            new()
            {
                Title = "فضاهای کاری",
                Category = "فضای کاری",
                Icon = "fa-solid fa-layer-group",
                Controller = "Workspace",
                Action = "Index",
                RequiresContext = ContextRequirementType.None
            },
            new()
            {
                Title = "پروژه‌ها",
                Category = "فضای کاری",
                Icon = "fa-solid fa-folder-open",
                Controller = "Project",
                Action = "Index",
                RequiresContext = ContextRequirementType.Workspace
            },
            new()
            {
                Title = "تیم‌ها",
                Category = "فضای کاری",
                Icon = "fa-solid fa-users",
                Controller = "Team",
                Action = "Index",
                RequiresContext = ContextRequirementType.Workspace
            },
            new()
            {
                Title = "اعضای فضای کاری",
                Category = "فضای کاری",
                Icon = "fa-solid fa-user-group",
                Controller = "WorkspaceMember",
                Action = "Index",
                RequiresContext = ContextRequirementType.Workspace
            },

            // ===== پروژه فعال =====
            new()
            {
                Title = "نمای کلی پروژه",
                Category = "پروژه فعال",
                Icon = "fa-solid fa-gauge-high",
                Controller = "ProjectDashboard",
                Action = "Index",
                RequiresContext = ContextRequirementType.Project
            },
            new()
            {
                Title = "برد پروژه",
                Category = "پروژه فعال",
                Icon = "fa-solid fa-square-check",
                Controller = "TaskBoard",
                Action = "Index",
                RequiresContext = ContextRequirementType.Project
            },
            new()
            {
                Title = "بک‌لاگ",
                Category = "پروژه فعال",
                Icon = "fa-solid fa-list-check",
                Controller = "Backlog",
                Action = "Index",
                RequiresContext = ContextRequirementType.Project
            },
            new()
            {
                Title = "اسپرینت‌ها",
                Category = "پروژه فعال",
                Icon = "fa-solid fa-bolt",
                Controller = "Sprint",
                Action = "Index",
                RequiresContext = ContextRequirementType.Project
            },
            new()
            {
                Title = "بیشتر",
                Category = "پروژه فعال",
                Icon = "fa-solid fa-ellipsis",
                RequiresContext = ContextRequirementType.Project,
                Children = new()
                {
                    new() { Title = "داشبورد چابک", Icon = "fa-solid fa-chart-simple", Controller = "AgileDashboard", Action = "Index", RequiresContext = ContextRequirementType.Project },
                    new() { Title = "معامله وظیفه", Icon = "fa-solid fa-right-left", Controller = "TaskTrade", Action = "Index", RequiresContext = ContextRequirementType.Project },
                    new() { Title = "خارج از مسیر", Icon = "fa-solid fa-road-circle-exclamation", Controller = "Offroad", Action = "Index", RequiresContext = ContextRequirementType.Project },
                    new() { Title = "برچسب‌ها", Icon = "fa-solid fa-tags", Controller = "Label", Action = "Index", RequiresContext = ContextRequirementType.Project },
                    new() { Title = "اعضای پروژه", Icon = "fa-solid fa-user-plus", Controller = "ProjectMember", Action = "Index", RequiresContext = ContextRequirementType.Project },
                }
            },

            // ===== مدیریت =====
            new()
            {
                Title = "گفتگو",
                Category = "مدیریت",
                Icon = "fa-solid fa-comments",
                Controller = "Chat",
                Action = "Index",
                RequiresContext = ContextRequirementType.None
            },

            // ===== گزارش‌ها =====
            new()
            {
                Title = "گزارش‌ها",
                Category = "گزارش‌ها",
                Icon = "fa-solid fa-chart-pie",
                RequiresContext = ContextRequirementType.None,
                Children = new()
                {
                    // --- سطح Workspace ---
                    new()
                    {
                        Title = "داشبورد فضای کاری",
                        Icon = "fa-solid fa-gauge",
                        Controller = "WorkspaceDashboard",
                        Action = "Index",
                        RequiresContext = ContextRequirementType.Workspace
                    },
                    new()
                    {
                        Title = "گزارش فضای کاری",
                        Icon = "fa-solid fa-chart-line",
                        Controller = "WorkspaceReport",
                        Action = "Index",
                        RequiresContext = ContextRequirementType.Workspace
                    },

                    // --- سطح Project ---
                    new()
                    {
                        Title = "گزارش پروژه",
                        Icon = "fa-solid fa-chart-area",
                        Controller = "ProjectReport",
                        Action = "Index",
                        RequiresContext = ContextRequirementType.Project
                    },
                    new()
                    {
                        Title = "گزارش اسپرینت",
                        Icon = "fa-solid fa-chart-simple",
                        Controller = "SprintReport",
                        Action = "Index",
                        RequiresContext = ContextRequirementType.Project
                    },
                    new()
                    {
                        Title = "ریسک تأخیر",
                        Icon = "fa-solid fa-triangle-exclamation",
                        Controller = "DelayRisk",
                        Action = "Index",
                        RequiresContext = ContextRequirementType.Project
                    },
                    new()
                    {
                        Title = "بار کاری",
                        Icon = "fa-solid fa-scale-balanced",
                        Controller = "Workload",
                        Action = "Index",
                        RequiresContext = ContextRequirementType.Project
                    },
                    new()
                    {
                        Title = "وابستگی‌ها",
                        Icon = "fa-solid fa-diagram-project",
                        Controller = "Dependency",
                        Action = "Index",
                        RequiresContext = ContextRequirementType.Project
                    }
                }
            },
            // ===== سیستم =====
            new()
            {
                Title = "داشبورد مدیریت",
                Category = "سیستم",
                Icon = "fa-solid fa-shield-halved",
                Controller = "Admin",
                Action = "Index",
                RequiredRole = "Admin",
                RequiresContext = ContextRequirementType.None
            }
        };
    }
}