using SmartTask.Web.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace SmartTask.Web.Services.Implementations
{
    /// <summary>
    /// ارسال Push Notification پیام چت به سایر اعضای پروژه از طریق Webpushr.
    /// </summary>
    public class WebpushrService : IWebpushrService
    {
        private readonly IChatService _chatService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public WebpushrService(
            IChatService chatService,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _chatService = chatService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendChatMessagePushAsync(
            int projectId,
            int senderUserId,
            string senderName,
            string content)
        {
            try
            {
                var members =
                    await _chatService.GetMembersAsync(projectId);

                foreach (var member in members)
                {
                    // فرستنده نیازی به اعلان پیام خودش ندارد
                    if (member.UserId == senderUserId)
                        continue;

                    if (!member.WebpushrSubscriberId.HasValue ||
                        member.WebpushrSubscriberId.Value <= 0)
                    {
                        continue;
                    }

                    try
                    {
                        var baseUrl = _configuration["App:BaseUrl"];

                        if (string.IsNullOrWhiteSpace(baseUrl))
                        {
                            Console.WriteLine("App:BaseUrl is missing.");
                            continue;
                        }

                        var url =
                            $"{baseUrl.TrimEnd('/')}/Chat?projectId={projectId}";

                        await SendWebpushrNotification(
                            member.WebpushrSubscriberId.Value,
                            Truncate(senderName, 100),
                            Truncate(content, 255),
                            url);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Webpushr notification failed for user {member.UserId}: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                // هرگز نباید پیام چت به‌خاطر شکست Push خراب شود
                Console.WriteLine(
                    $"Webpushr member lookup failed: {ex}");
            }
        }

        public async Task SendWebpushrNotification(
            long subscriberId,
            string title,
            string message,
            string url)
        {
            var key = _configuration["Webpushr:Key"];

            var authToken = _configuration["Webpushr:AuthToken"];

            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(authToken))
            {
                Console.WriteLine(
                    "Webpushr Key/AuthToken is missing.");
                return;
            }

            var payload = new
            {
                title = title,
                message = message,
                target_url = url,
                sid = subscriberId
            };

            var json = JsonSerializer.Serialize(payload);

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.webpushr.com/v1/notification/send/sid");

            request.Headers.Add(
                "webpushrKey",
                key);

            request.Headers.Add(
                "webpushrAuthToken",
                authToken);

            request.Content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

            var client =
                _httpClientFactory.CreateClient();

            using var response =
                await client.SendAsync(request);

            var result =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Webpushr notification sent to SID {subscriberId}");
            }
            else
            {
                Console.WriteLine(
                    $"Webpushr HTTP {(int)response.StatusCode}");
                Console.WriteLine(result);
            }
        }

        public async Task SendTestPushAsync(
            int projectId,
            int senderUserId,
            string senderName)
        {
            try
            {
                var members =
                    await _chatService.GetMembersAsync(projectId);

                var baseUrl = _configuration["App:BaseUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    Console.WriteLine("App:BaseUrl is missing.");
                    return;
                }

                var url =
                    $"{baseUrl.TrimEnd('/')}/Chat?projectId={projectId}";

                // برخلاف پیام عادی، فرستنده هم اعلان می‌گیرد تا تست سریع باشد
                foreach (var member in members)
                {
                    if (!member.WebpushrSubscriberId.HasValue ||
                        member.WebpushrSubscriberId.Value <= 0)
                    {
                        continue;
                    }

                    try
                    {
                        await SendWebpushrNotification(
                            member.WebpushrSubscriberId.Value,
                            "SmartTask - اعلان آزمایشی",
                            $"این یک اعلان آزمایشی است؛ توسط {senderName} ارسال شد.",
                            url);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Webpushr test push failed for user {member.UserId}: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Webpushr test push member lookup failed: {ex}");
            }
        }

        /// <summary>
        /// محدودیت طول پارامترهای API وب‌پوش؛ متن اضافه را با ellipsis کوتاه می‌کند.
        /// </summary>
        private static string Truncate(
            string value,
            int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - 3) + "...";
        }
    
    
    }
}
