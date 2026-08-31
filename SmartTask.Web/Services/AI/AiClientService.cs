using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace SmartTask.Web.Services.AI;

public class AiClientService : IAiClientService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiSettings _settings;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public AiClientService(HttpClient httpClient, IOptions<OpenAiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }
	
    public async Task<string> GetCompletionAsync(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.7,
        CancellationToken cancellationToken = default)
    {
    try
	{	        
        var requestBody = new
        {
            model = _settings.Model,
            temperature,
            max_tokens = 4096,
            stop = new[] { "```" },
            messages = new object[]
            {
                new { role = "system", content = systemPrompt + "\n\n/no_think" },
                new { role = "user", content = userPrompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(120));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("درخواست به مدل بیش از ۶۰ ثانیه طول کشید.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"LM Studio API error ({(int)response.StatusCode}): {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);
        var message = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message");

        // Qwen3 reasoning models: content might be empty, reasoning_content has the thinking
        var content = message.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null;
        
        // If content is empty or null, try reasoning_content (Qwen3 reasoning model)
        if (string.IsNullOrWhiteSpace(content) && message.TryGetProperty("reasoning_content", out var reasoningProp))
        {
            content = reasoningProp.GetString();
        }

        return content ?? string.Empty;

        }
        catch (Exception ex)
        {

            throw ex;
        }
    }

    public async Task<T?> GetStructuredCompletionAsync<T>(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.5,
        CancellationToken cancellationToken = default)
    {
        // System prompt رو تقویت می‌کنیم که LLM فقط JSON برگردونه
        var structuredSystemPrompt = systemPrompt +
            "\n\nمهم: خروجی باید فقط JSON معتبر باشد. هیچ متن اضافی، Markdown، یا تگی قبل یا بعد از JSON نباشد. " +
            "فقط آبجکت JSON را بازگردان.";

        var rawResponse = await GetCompletionAsync(structuredSystemPrompt, userPrompt, temperature, cancellationToken);

        // تلاش برای Parse کردن JSON خروجی
        var cleaned = rawResponse.Trim();

        // حذف Markdown code fence اگر وجود داشته باشه
        if (cleaned.StartsWith("```"))
        {
            var firstNewLine = cleaned.IndexOf('\n');
            var lastFence = cleaned.LastIndexOf("```");
            if (firstNewLine >= 0 && lastFence > firstNewLine)
                cleaned = cleaned.Substring(firstNewLine + 1, lastFence - firstNewLine - 1).Trim();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(cleaned, _jsonOptions);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AiClientService] JSON parse failed: {ex.Message}. Raw: {cleaned.Substring(0, Math.Min(200, cleaned.Length))}");
            return default;
        }
    }
}
