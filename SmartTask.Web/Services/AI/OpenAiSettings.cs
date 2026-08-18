namespace SmartTask.Web.Services.AI;

public class OpenAiSettings
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "qwen/qwen3-4b";
    public string BaseUrl { get; set; } = "http://92.246.145.99:1234/v1/chat/completions";
}
