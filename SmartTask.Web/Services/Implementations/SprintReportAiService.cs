using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.SprintReport;
using SmartTask.Web.Services.AI;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class SprintReportAiService : ISprintReportAiService
{
    private readonly ApplicationDbContext _context;
    private readonly IAiClientService _aiClient;

    public SprintReportAiService(ApplicationDbContext context, IAiClientService aiClient)
    {
        _context = context;
        _aiClient = aiClient;
    }

    public async Task<List<SprintReportViewModel>> GetReportsAsync(int sprintId)
    {
        return await _context.SprintReports
            .Where(x => x.SprintId == sprintId && x.ViewState)
            .Include(x => x.GeneratedByUser)
            .OrderByDescending(x => x.GeneratedDate)
            .Select(x => new SprintReportViewModel
            {
                Id = x.Id,
                Content = x.Content,
                GeneratedByName = x.GeneratedByUser.FullName,
                GeneratedDate = x.GeneratedDate
            })
            .ToListAsync();
    }

    public async Task<SprintReportViewModel> GenerateReportAsync(int sprintId, int currentUserId)
    {
        var sprint = await _context.Sprints
            .Include(x => x.Project)
            .Include(x => x.UserStories.Where(s => s.ViewState))
            .FirstOrDefaultAsync(x => x.Id == sprintId && x.ViewState);

        if (sprint == null)
            throw new InvalidOperationException("اسپرینت یافت نشد.");

        var storyIds = sprint.UserStories.Select(s => s.Id).ToList();

        var totalPoints = sprint.UserStories.Sum(s => s.StoryPoint);
        var completedPoints = sprint.UserStories
            .Where(s => s.Status == StoryStatusType.Done)
            .Sum(s => s.StoryPoint);
        var completionRate = totalPoints == 0 ? 0 : (int)Math.Round((double)completedPoints / totalPoints * 100);

        var notDoneStories = sprint.UserStories
            .Where(s => s.Status != StoryStatusType.Done)
            .Select(s => s.Title)
            .ToList();

        var tasks = await _context.TaskItems
            .Where(t => storyIds.Contains(t.UserStoryId) && t.ViewState)
            .Include(t => t.Assignments)
            .ToListAsync();

        var totalTasks = tasks.Count;
        var doneTasks = tasks.Count(t => t.Status == TaskStatusType.Done);

        var contributorIds = tasks
            .Where(t => t.Status == TaskStatusType.Done)
            .SelectMany(t => t.Assignments)
            .GroupBy(a => a.ApplicationUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(3)
            .ToList();

        var contributorNames = new List<string>();
        foreach (var c in contributorIds)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == c.UserId);
            if (user != null)
                contributorNames.Add($"{user.FullName} ({c.Count} Task)");
        }

        var systemPrompt =
            "تو یک اسکرام‌مستر باتجربه هستی که در پایان هر اسپرینت، یک گزارش خلاصه برای تیم و مدیران می‌نویسی. " +
            "گزارش باید به زبان فارسی، دوستانه و حرفه‌ای، شامل ۳ تا ۵ جمله باشد. " +
            "ابتدا دستاوردهای اسپرینت را برجسته کن، بعد اگر موردی ناتمام مانده به‌طور سازنده اشاره کن، " +
            "و در پایان یک جمله انگیزشی یا پیشنهاد برای اسپرینت بعدی بده. " +
            "فقط متن گزارش را بازگردان، بدون Markdown، بدون عنوان یا فهرست اضافه.";

        var userPrompt =
            $"نام اسپرینت: {sprint.Name}\n" +
            $"پروژه: {sprint.Project.Name}\n" +
            $"هدف اسپرینت: {(string.IsNullOrWhiteSpace(sprint.Goal) ? "تعیین نشده" : sprint.Goal)}\n" +
            $"بازه زمانی: {sprint.StartDate:yyyy/MM/dd} تا {sprint.EndDate:yyyy/MM/dd}\n" +
            $"Story Point برنامه‌ریزی‌شده: {totalPoints}\n" +
            $"Story Point تکمیل‌شده: {completedPoints} ({completionRate}%)\n" +
            $"Task های تکمیل‌شده: {doneTasks} از {totalTasks}\n" +
            (notDoneStories.Any()
                ? $"User Story های ناتمام: {string.Join("، ", notDoneStories.Take(5))}\n"
                : "همه User Story های این اسپرینت تکمیل شدند.\n") +
            (contributorNames.Any()
                ? $"بیشترین مشارکت: {string.Join("، ", contributorNames)}\n"
                : "") +
            "\nلطفاً گزارش پایان این اسپرینت را بنویس.";

        var content = await _aiClient.GetCompletionAsync(systemPrompt, userPrompt, temperature: 0.7);

        var report = new SprintReport
        {
            SprintId = sprintId,
            Content = content,
            GeneratedByUserId = currentUserId,
            GeneratedDate = DateTime.Now,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        _context.SprintReports.Add(report);
        await _context.SaveChangesAsync();

        return new SprintReportViewModel
        {
            Id = report.Id,
            Content = report.Content,
            GeneratedByName = (await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId))?.FullName ?? "-",
            GeneratedDate = report.GeneratedDate
        };
    }
}