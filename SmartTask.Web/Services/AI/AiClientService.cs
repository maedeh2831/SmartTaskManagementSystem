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
            reasoning_effort = "none",
            max_tokens = 1024,
            stop = new[] { "```" },
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
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
        cts.CancelAfter(TimeSpan.FromSeconds(60));

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
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;

        }
        catch (Exception ex)
        {

            throw ex;
        }
    }
}
