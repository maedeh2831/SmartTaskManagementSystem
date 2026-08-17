namespace SmartTask.Web.Services.Interfaces
{
    /// <summary>
    /// ارسال Push Notification پیام‌های چت به سایر اعضای پروژه از طریق Webpushr.
    /// </summary>
    public interface IWebpushrService
    {
        /// <summary>
        /// برای همه اعضای پروژه (به‌جز فرستنده) که شناسه مشترک Webpushr دارند، اعلان ارسال می‌کند.
        /// شکست در ارسال هر اعلان هرگز پیام چت را خراب نمی‌کند.
        /// </summary>
        Task SendChatMessagePushAsync(
            int projectId,
            int senderUserId,
            string senderName,
            string content);

        /// <summary>ارسال یک اعلان به یک شناسه مشترک مشخص.</summary>
        Task SendWebpushrNotification(
            long subscriberId,
            string title,
            string message,
            string url);

        /// <summary>
        /// ارسال اعلان آزمایشی به همه اعضای پروژه (شامل فرستنده) برای تست تحویل پوش؛
        /// بدون اینکه پیامی در چت ذخیره شود.
        /// </summary>
        Task SendTestPushAsync(
            int projectId,
            int senderUserId,
            string senderName);
    }
}
