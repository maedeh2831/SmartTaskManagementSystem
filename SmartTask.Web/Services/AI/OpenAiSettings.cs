namespace SmartTask.Web.Services.AI;

public class OpenAiSettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = "gpt-4o-mini";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
}