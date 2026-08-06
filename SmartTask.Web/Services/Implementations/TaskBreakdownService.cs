using System.Text.Json;
using SmartTask.Web.Services.AI;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class TaskBreakdownService : ITaskBreakdownService
{
    private readonly ITaskService _taskService;
    private readonly IAiClientService _aiClient;

    public TaskBreakdownService(ITaskService taskService, IAiClientService aiClient)
    {
        _taskService = taskService;
        _aiClient = aiClient;
    }

    public async Task<List<string>> GenerateSubTasksAsync(int taskId)
    {
        var task = await _taskService.GetDetailsAsync(taskId);

        if (task == null)
            throw new InvalidOperationException("Task یافت نشد.");

        var systemPrompt =
            "تو یک دستیار مدیریت پروژه نرم‌افزاری هستی. وظیفه‌ات این است که یک Task را به زیروظایف (SubTask) عملی و قابل‌اجرا تجزیه کنی. " +
            "خروجی را فقط و فقط به‌صورت یک آرایه JSON از رشته‌ها بازگردان، بدون هیچ توضیح اضافه، بدون Markdown، بدون ```. " +
            "هر رشته باید عنوان کوتاه یک زیروظیفه به زبان فارسی باشد (حداکثر ۸ کلمه). بین ۳ تا ۷ زیروظیفه پیشنهاد بده.";

        var userPrompt =
            $"عنوان Task: {task.Title}\n" +
            $"توضیحات: {(string.IsNullOrWhiteSpace(task.Description) ? "ندارد" : task.Description)}\n" +
            $"نوع: {task.Type}\n" +
            $"اولویت: {task.Priority}\n\n" +
            "لطفاً زیروظایف پیشنهادی را برای انجام این Task تولید کن.";

        var rawResponse = await _aiClient.GetCompletionAsync(systemPrompt, userPrompt, temperature: 0.5);

        return ParseSubTaskTitles(rawResponse);
    }

    private static List<string> ParseSubTaskTitles(string raw)
    {
        var cleaned = raw.Trim();

        if (cleaned.StartsWith("```"))
        {
            var firstNewLine = cleaned.IndexOf('\n');
            var lastFence = cleaned.LastIndexOf("```");
            if (firstNewLine >= 0 && lastFence > firstNewLine)
                cleaned = cleaned.Substring(firstNewLine + 1, lastFence - firstNewLine - 1).Trim();
        }

        try
        {
            var titles = JsonSerializer.Deserialize<List<string>>(cleaned);
            return titles?
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Take(7)
                .ToList() ?? new List<string>();
        }
        catch
        {
            return cleaned
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim(' ', '-', '*', '•', '.', '\t'))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Take(7)
                .ToList();
        }
    }
}