namespace SmartTask.Web.Infrastructure.Interfaces
{
    /// <summary>
    /// ردیابی وضعیت آنلاین/آفلاین کاربران بر اساس اتصال‌های فعال SignalR.
    /// </summary>
    public interface IPresenceTracker
    {
        /// <summary>ثبت اتصال جدید. اگر اولین اتصال کاربر باشد true برمی‌گرداند (یعنی کاربر آنلاین شد).</summary>
        bool Connect(int userId, string connectionId);

        /// <summary>حذف اتصال. اگر آخرین اتصال کاربر باشد true برمی‌گرداند (یعنی کاربر آفلاین شد).</summary>
        bool Disconnect(int userId, string connectionId);

        bool IsOnline(int userId);

        IReadOnlyCollection<int> GetOnlineUsers();

        HashSet<int> FilterOnline(IEnumerable<int> userIds);

        DateTime? GetLastSeen(int userId);
    }
}
