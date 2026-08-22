using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Settings;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;

        // فقط این ۶ نوع، توی Settings قابل تنظیم توسط کاربر هستن
        private static readonly Dictionary<NotificationType, string> ConfigurableNotifications = new()
        {
            { NotificationType.Assignment, "وظیفه جدید به من اختصاص داده شد" },
            { NotificationType.StatusChange, "تغییر وضعیت Task" },
            { NotificationType.Mention, "منشن شدن در کامنت" },
            { NotificationType.Comment, "کامنت جدید روی Task من" },
            { NotificationType.Deadline, "نزدیک شدن Deadline" },
            { NotificationType.Invitation, "دعوت به Workspace" },
        };

        public SettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SettingsViewModel> GetSettingsAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new InvalidOperationException("کاربر یافت نشد.");

            var savedPreferences = await _context.UserNotificationPreferences
                .Where(x => x.ApplicationUserId == userId)
                .ToListAsync();

            var notifications = ConfigurableNotifications.Select(kv => new NotificationPreferenceItemViewModel
            {
                NotificationType = kv.Key,
                Title = kv.Value,
                IsEnabled = savedPreferences.FirstOrDefault(p => p.NotificationType == kv.Key)?.IsEnabled ?? true
            }).ToList();

            var workspaces = await _context.WorkspaceMembers
                .Where(wm => wm.ApplicationUserId == userId)
                .Select(wm => new SelectListItem
                {
                    Value = wm.WorkspaceId.ToString(),
                    Text = wm.Workspace.Name
                }).ToListAsync();

            return new SettingsViewModel
            {
                Account = new AccountSettingsViewModel
                {
                    TimeZone = user.TimeZone,
                    DateFormat = user.DateFormat,
                },
                Appearance = new AppearanceSettingsViewModel
                {
                    Theme = user.Theme,
                    TaskDensity = user.TaskDensity
                },
                Notifications = notifications,
                Workspace = new WorkspaceSettingsViewModel
                {
                    DefaultWorkspaceId = user.DefaultWorkspaceId,
                    Workspaces = workspaces
                },
                Management = new ManagementSettingsViewModel
                {
                    AutoCascadeDependencyDates = user.AutoCascadeDependencyDates
                }
            };
        }

        public async Task UpdateAccountAsync(int userId, AccountSettingsViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new InvalidOperationException("کاربر یافت نشد.");

            user.TimeZone = model.TimeZone;
            user.DateFormat = model.DateFormat;
            user.ChangeDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAppearanceAsync(int userId, AppearanceSettingsViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new InvalidOperationException("کاربر یافت نشد.");

            user.Theme = model.Theme;
            user.TaskDensity = model.TaskDensity;
            user.ChangeDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateNotificationsAsync(int userId, List<NotificationPreferenceItemViewModel> model)
        {
            var existing = await _context.UserNotificationPreferences
                .Where(x => x.ApplicationUserId == userId)
                .ToListAsync();

            foreach (var item in model)
            {
                var pref = existing.FirstOrDefault(x => x.NotificationType == item.NotificationType);

                if (pref == null)
                {
                    await _context.UserNotificationPreferences.AddAsync(new UserNotificationPreference
                    {
                        ApplicationUserId = userId,
                        NotificationType = item.NotificationType,
                        IsEnabled = item.IsEnabled
                    });
                }
                else
                {
                    pref.IsEnabled = item.IsEnabled;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateDefaultWorkspaceAsync(int userId, int? workspaceId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new InvalidOperationException("کاربر یافت نشد.");

            user.DefaultWorkspaceId = workspaceId;
            user.ChangeDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateManagementAsync(int userId, ManagementSettingsViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId)
                ?? throw new InvalidOperationException("کاربر یافت نشد.");

            user.AutoCascadeDependencyDates = model.AutoCascadeDependencyDates;
            user.ChangeDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}