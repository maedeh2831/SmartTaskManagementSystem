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

    public async Task<List<string>> GenerateSubTasksAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskService.GetDetailsAsync(taskId);
        if (task == null)
            throw new InvalidOperationException("Task یافت نشد.");

        const string systemPrompt =
         "/no_think فقط JSON array از رشته فارسی (۳ تا ۷ آیتم،هرکدام حداکثر ۸ کلمه) از زیروظایف Task برگردان. بدون توضیح، بدون Markdown.";

        var userPrompt =
            $"/no_think Task: {task.Title} | {(string.IsNullOrWhiteSpace(task.Description) ? "" : task.Description)} | نوع: {task.Type} | اولویت: {task.Priority}";

        var rawResponse = await _aiClient.GetCompletionAsync(systemPrompt, userPrompt, temperature: 0.5, cancellationToken);
        return ParseSubTaskTitles(rawResponse);
    }

    private static readonly JsonSerializerOptions _parseOptions = new(JsonSerializerDefaults.Web);

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
            var titles = JsonSerializer.Deserialize<List<string>>(cleaned, _parseOptions);
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