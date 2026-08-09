namespace SmartTask.Web.Services.AI;

public class OpenAiSettings
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "qwen/qwen3-4b";
    public string BaseUrl { get; set; } = "http://publicIp/v1/chat/completions";
}
