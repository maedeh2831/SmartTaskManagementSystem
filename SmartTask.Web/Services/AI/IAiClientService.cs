using System.Text.Json;

namespace SmartTask.Web.Services.AI;

public interface IAiClientService
{
    Task<string> GetCompletionAsync(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ارسال درخواست به LLM و دریافت خروجی JSON ساختاریافته.
    /// خروجی LLM رو Parse کرده و به کلاس مورد نظر تبدیل می‌کنه.
    /// </summary>
    Task<T?> GetStructuredCompletionAsync<T>(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.5,
        CancellationToken cancellationToken = default);
}