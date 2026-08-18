namespace SmartTask.Web.Services.AI;

public interface IAiClientService
{
    Task<string> GetCompletionAsync(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.7,
        CancellationToken cancellationToken = default);
}