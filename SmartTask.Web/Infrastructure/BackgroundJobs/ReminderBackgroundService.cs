using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Infrastructure.BackgroundJobs
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

        private const string Marker24h = "یادآوری خودکار: ۲۴ ساعت مانده به موعد";
        private const string Marker1h = "یادآوری خودکار: ۱ ساعت مانده به موعد";

        public ReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessManualRemindersAsync();
                    await ProcessDeadlineRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در اجرای ReminderBackgroundService");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ProcessManualRemindersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var pending = await reminderService.GetPendingManualRemindersAsync();

            foreach (var reminder in pending)
            {
                await notificationService.CreateAsync(
                    reminder.ApplicationUserId,
                    "یادآوری",
                    $"{reminder.Title} (Task: {reminder.TaskItem.Title})",
                    NotificationType.Reminder);

                await reminderService.MarkAsSentAsync(reminder.Id);
            }
        }

        private async Task ProcessDeadlineRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.Now;

            var tasks = await context.TaskItems
                .Where(x =>
                    x.ViewState &&
                    x.DueDate.HasValue &&
                    x.Status != TaskStatusType.Done &&
                    x.Status != TaskStatusType.Cancelled &&
                    x.DueDate.Value > now)
                .Include(x => x.Assignments.Where(a => a.ViewState))
                .ToListAsync(stoppingToken);

            foreach (var task in tasks)
            {
                var hoursLeft = (task.DueDate!.Value - now).TotalHours;

                if (hoursLeft <= 24 && hoursLeft > 1)
                {
                    await SendDeadlineNotificationIfNeededAsync(task, reminderService, notificationService, Marker24h, "24 ساعت");
                }
                else if (hoursLeft <= 1 && hoursLeft > 0)
                {
                    await SendDeadlineNotificationIfNeededAsync(task, reminderService, notificationService, Marker1h, "1 ساعت");
                }
            }
        }

        private async Task SendDeadlineNotificationIfNeededAsync(
            TaskItem task,
            IReminderService reminderService,
            INotificationService notificationService,
            string marker,
            string label)
        {
            foreach (var assignment in task.Assignments)
            {
                var alreadySent = await reminderService.AutoReminderExistsAsync(task.Id, assignment.ApplicationUserId, marker);
                if (alreadySent)
                    continue;

                await notificationService.CreateAsync(
                    assignment.ApplicationUserId,
                    "نزدیک شدن موعد Task",
                    $"موعد Task «{task.Title}» تا {label} دیگر می‌رسد.",
                    NotificationType.Deadline);

                await reminderService.CreateAutoSentReminderAsync(
                    task.Id, assignment.ApplicationUserId, marker, task.DueDate!.Value);
            }
        }
    }
}