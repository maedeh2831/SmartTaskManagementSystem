using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Infrastructure.BackgroundJobs
{
    public class OverdueCascadeBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueCascadeBackgroundService> _logger;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(30);

        public OverdueCascadeBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OverdueCascadeBackgroundService> logger)
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
                    await ProcessOverdueCascadeAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در اجرای OverdueCascadeBackgroundService");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ProcessOverdueCascadeAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dependencyService = scope.ServiceProvider.GetRequiredService<ITaskDependencyService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var activityLogService = scope.ServiceProvider.GetRequiredService<IActivityLogService>();

            var now = DateTime.Now;

            var overdueTasks = await context.TaskItems
                .Where(x =>
                    x.ViewState &&
                    x.DueDate.HasValue &&
                    x.DueDate.Value.Date < now.Date &&
                    x.Status != TaskStatusType.Done &&
                    x.Status != TaskStatusType.Cancelled)
                .ToListAsync(stoppingToken);

            foreach (var sourceTask in overdueTasks)
            {
                var delayDays = (now.Date - sourceTask.DueDate!.Value.Date).Days;
                if (delayDays <= 0)
                    continue;

                var impactedChain = await dependencyService.GetImpactedTasksAsync(sourceTask.Id, delayDays);
                var requiredChain = impactedChain.Where(x => x.IsRequiredChain).ToList();

                foreach (var impacted in requiredChain)
                {
                    await ApplyCascadeAsync(
                        context, notificationService, activityLogService,
                        sourceTask, impacted.TaskId, delayDays);
                }
            }
        }

        private async Task ApplyCascadeAsync(
            ApplicationDbContext context,
            INotificationService notificationService,
            IActivityLogService activityLogService,
            TaskItem sourceTask,
            int impactedTaskId,
            int currentDelayDays)
        {
            var impactedTask = await context.TaskItems
                .Include(x => x.Assignments.Where(a => a.ViewState))
                .FirstOrDefaultAsync(x => x.Id == impactedTaskId && x.ViewState);

            if (impactedTask == null)
                return;

            if (impactedTask.Status == TaskStatusType.Done || impactedTask.Status == TaskStatusType.Cancelled)
                return;

            var log = await context.OverdueCascadeLogs
                .FirstOrDefaultAsync(x => x.SourceTaskId == sourceTask.Id && x.ImpactedTaskId == impactedTaskId);

            int additionalDelay;

            if (log == null)
            {
                additionalDelay = currentDelayDays;

                context.OverdueCascadeLogs.Add(new OverdueCascadeLog
                {
                    SourceTaskId = sourceTask.Id,
                    ImpactedTaskId = impactedTaskId,
                    DelayDaysApplied = currentDelayDays,
                    AppliedDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    ViewState = true
                });
            }
            else if (currentDelayDays > log.DelayDaysApplied)
            {
                additionalDelay = currentDelayDays - log.DelayDaysApplied;
                log.DelayDaysApplied = currentDelayDays;
                log.AppliedDate = DateTime.Now;
            }
            else
            {
                return; // تأخیر جدیدی برای اعمال وجود ندارد
            }

            if (impactedTask.DueDate.HasValue)
                impactedTask.DueDate = impactedTask.DueDate.Value.AddDays(additionalDelay);

            impactedTask.ChangeDate = DateTime.Now;

            await context.SaveChangesAsync();

            foreach (var assignment in impactedTask.Assignments)
            {
                await notificationService.CreateAsync(
                    assignment.ApplicationUserId,
                    "تأخیر زنجیره‌ای",
                    $"به‌دلیل تأخیر در Task «{sourceTask.Title}»، موعد Task «{impactedTask.Title}» به‌طور خودکار {additionalDelay} روز به تعویق افتاد.",
                    NotificationType.Deadline);
            }

            await activityLogService.LogAsync(
                            impactedTask.Assignments.FirstOrDefault()?.ApplicationUserId ?? 5,
                            "Overdue Auto-Cascade",
                            $"موعد Task «{impactedTask.Title}» به‌دلیل تأخیر {additionalDelay} روزه در «{sourceTask.Title}» به‌صورت خودکار به‌روزرسانی شد.",
                            impactedTask.Id);
        }
    }
}