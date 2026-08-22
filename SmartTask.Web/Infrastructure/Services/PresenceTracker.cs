using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Infrastructure.Services
{
    /// <summary>
    /// نگهدارنده وضعیت حضور کاربران در حافظه. به‌صورت Singleton ثبت می‌شود.
    /// هر کاربر می‌تواند چند اتصال هم‌زمان داشته باشد (چند تب/چند دستگاه).
    /// </summary>
    public class PresenceTracker : IPresenceTracker
    {
        private readonly Dictionary<int, HashSet<string>> _connections = new();
        private readonly Dictionary<int, DateTime> _lastSeen = new();
        private readonly object _lock = new();

        public bool Connect(int userId, string connectionId)
        {
            lock (_lock)
            {
                if (_connections.TryGetValue(userId, out var set))
                {
                    set.Add(connectionId);
                    return false;
                }

                _connections[userId] = new HashSet<string> { connectionId };
                return true;
            }
        }

        public bool Disconnect(int userId, string connectionId)
        {
            lock (_lock)
            {
                if (!_connections.TryGetValue(userId, out var set))
                    return false;

                set.Remove(connectionId);

                if (set.Count > 0)
                    return false;

                _connections.Remove(userId);
                _lastSeen[userId] = DateTime.UtcNow;
                return true;
            }
        }

        public bool IsOnline(int userId)
        {
            lock (_lock)
            {
                return _connections.ContainsKey(userId);
            }
        }

        public IReadOnlyCollection<int> GetOnlineUsers()
        {
            lock (_lock)
            {
                return _connections.Keys.ToList();
            }
        }

        public HashSet<int> FilterOnline(IEnumerable<int> userIds)
        {
            lock (_lock)
            {
                return userIds.Where(_connections.ContainsKey).ToHashSet();
            }
        }

        public DateTime? GetLastSeen(int userId)
        {
            lock (_lock)
            {
                return _lastSeen.TryGetValue(userId, out var value) ? value : null;
            }
        }

        public IReadOnlyCollection<string> GetConnections(int userId)
        {
            lock (_lock)
            {
                return _connections.TryGetValue(userId, out var set)
                    ? set.ToList()
                    : Array.Empty<string>();
            }
        }
    }
}
