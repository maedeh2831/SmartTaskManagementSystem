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
         "/no_think\r\nبا توجه به عنوان، توضیحات، وضعیت، اولویت، وابستگی‌ها و سایر داده‌های Task، فقط زیروظایف اجرایی و قابل‌انجامی را پیشنهاد بده که مستقیماً برای تکمیل همین Task لازم یا بسیار مرتبط هستند.\r\n\r\nقوانین:\r\n- زیروظایف باید مستقیماً از محتوای همین Task استخراج یا استنباط منطقی شوند.\r\n- هر زیروظیفه باید یک کار واقعی، مشخص و قابل انجام باشد.\r\n- زیروظایف باید به انجام یا تکمیل Task کمک مستقیم کنند.\r\n- از پیشنهادهای کلی، مدیریتی، تزئینی یا غیرضروری خودداری کن.\r\n- زیروظایفی که صرفاً مرتبط با موضوع Task هستند ولی برای انجام آن ضروری یا مفید نیستند، برنگردان.\r\n- از تکرار یا بازنویسی عنوان Task به‌عنوان SubTask خودداری کن.\r\n- زیروظایف را از مرتبط‌ترین و ضروری‌ترین مورد به کم‌اهمیت‌تر مرتب کن.\r\n- اگر از داده‌های Task نتوان زیروظیفه معنادار و اجرایی استخراج کرد، حدس بی‌مورد نزن و فقط مواردی را برگردان که ارتباط مشخصی دارند.\r\n- بین ۳ تا ۷ زیروظیفه برگردان.\r\n- هر زیروظیفه حداکثر ۸ کلمه و به زبان فارسی باشد.\r\n- خروجی فقط JSON Array معتبر از رشته‌های فارسی باشد.\r\n- بدون توضیح، بدون Markdown و بدون متن اضافی.\r\n\r\nنمونه خروجی:\r\n[\"طراحی ساختار جدول کاربران\", \"پیاده‌سازی API ثبت کاربر\", \"اعتبارسنجی اطلاعات ورودی\", \"اتصال فرم ثبت‌نام به API\"]";

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