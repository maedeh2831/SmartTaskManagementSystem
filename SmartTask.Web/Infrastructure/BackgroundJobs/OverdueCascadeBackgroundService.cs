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
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            // OPTIMIZED: Load all overdue tasks with minimal data
            var overdueTasks = await context.TaskItems
                .Where(x =>
                    x.ViewState &&
                    x.DueDate.HasValue &&
                    x.DueDate.Value.Date < now.Date &&
                    x.Status != TaskStatusType.Done &&
                    x.Status != TaskStatusType.Cancelled)
                .Select(x => new { x.Id, x.Title, x.DueDate })
                .ToListAsync(stoppingToken);

            if (overdueTasks.Count == 0)
                return;

            // OPTIMIZED: Load all cascade logs upfront instead of querying per task
            var sourceTaskIds = overdueTasks.Select(t => t.Id).ToList();
            var cascadeLogs = await context.OverdueCascadeLogs
                .Where(x => x.ViewState && sourceTaskIds.Contains(x.SourceTaskId))
                .ToListAsync(stoppingToken);

            // OPTIMIZED: Pre-compute cascade log dictionary for O(1) lookups
            var cascadeLogDict = cascadeLogs
                .GroupBy(x => new { x.SourceTaskId, x.ImpactedTaskId })
                .ToDictionary(
                    g => (g.Key.SourceTaskId, g.Key.ImpactedTaskId),
                    g => g.First());

            var cascadesToApply = new List<(TaskItem sourceTask, int impactedTaskId, int delayDays)>();

            foreach (var sourceTask in overdueTasks)
            {
                var delayDays = (now.Date - sourceTask.DueDate!.Value.Date).Days;
                if (delayDays <= 0)
                    continue;

                var impactedChain = await dependencyService.GetImpactedTasksAsync(sourceTask.Id, delayDays);
                var requiredChain = impactedChain.Where(x => x.IsRequiredChain).ToList();

                foreach (var impacted in requiredChain)
                {
                    cascadesToApply.Add((
                        new TaskItem { Id = sourceTask.Id, Title = sourceTask.Title, DueDate = sourceTask.DueDate },
                        impacted.TaskId,
                        delayDays));
                }
            }

            if (cascadesToApply.Count == 0)
                return;

            // OPTIMIZED: Load all impacted tasks at once instead of individually
            var impactedTaskIds = cascadesToApply.Select(x => x.impactedTaskId).Distinct().ToList();
            var impactedTasks = await context.TaskItems
                .Where(x => x.ViewState && impactedTaskIds.Contains(x.Id))
                .Include(x => x.Assignments.Where(a => a.ViewState))
                .ToListAsync(stoppingToken);

            var impactedTaskDict = impactedTasks.ToDictionary(x => x.Id);

            var notificationTasks = new List<Task>();
            var activityLogTasks = new List<Task>();

            foreach (var (sourceTask, impactedTaskId, currentDelayDays) in cascadesToApply)
            {
                if (!impactedTaskDict.TryGetValue(impactedTaskId, out var impactedTask))
                    continue;

                if (impactedTask.Status == TaskStatusType.Done || impactedTask.Status == TaskStatusType.Cancelled)
                    continue;

                // OPTIMIZED: Use O(1) lookup instead of querying
                var logKey = (sourceTask.Id, impactedTaskId);
                var logExists = cascadeLogDict.TryGetValue(logKey, out var log);

                int additionalDelay;

                if (!logExists)
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
                    continue; // تأخیر جدیدی برای اعمال وجود ندارد
                }

                if (impactedTask.DueDate.HasValue)
                    impactedTask.DueDate = impactedTask.DueDate.Value.AddDays(additionalDelay);

                impactedTask.ChangeDate = DateTime.Now;

                // OPTIMIZED: Batch notifications using Task.WhenAll
                foreach (var assignment in impactedTask.Assignments)
                {
                    notificationTasks.Add(notificationService.CreateAsync(
                        assignment.ApplicationUserId,
                        "تأخیر زنجیره‌ای",
                        $"به‌دلیل تأخیر در Task «{sourceTask.Title}»، موعد Task «{impactedTask.Title}» به‌طور خودکار {additionalDelay} روز به تعویق افتاد.",
                        NotificationType.Deadline));
                }

                var assignee = impactedTask.Assignments.FirstOrDefault();
                if (assignee != null)
                {
                    activityLogTasks.Add(activityLogService.LogAsync(
                        assignee.ApplicationUserId,
                        "Overdue Auto-Cascade",
                        $"موعد Task «{impactedTask.Title}» به‌دلیل تأخیر {additionalDelay} روزه در «{sourceTask.Title}» به‌صورت خودکار به‌روزرسانی شد.",
                        impactedTask.Id));
                }
            }

            // Save all cascades at once
            await context.SaveChangesAsync(stoppingToken);

            // OPTIMIZED: Execute all notifications in parallel
            if (notificationTasks.Any())
                await Task.WhenAll(notificationTasks);

            // OPTIMIZED: Execute all activity logs in parallel
            if (activityLogTasks.Any())
                await Task.WhenAll(activityLogTasks);
        }
    }
}
