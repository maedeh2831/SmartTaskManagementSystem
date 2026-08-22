using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class UserSessionTracker : IUserSessionTracker
    {
        private readonly ApplicationDbContext _context;

        public UserSessionTracker(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserSession> TrackLoginAsync(int userId, string userAgent, string? ipAddress, HttpContext httpContext)
        {
            var (deviceInfo, os) = ParseUserAgent(userAgent);

            var session = new UserSession
            {
                ApplicationUserId = userId,
                SessionToken = Guid.NewGuid().ToString("N"),
                DeviceInfo = deviceInfo,
                OperatingSystem = os,
                IpAddress = ipAddress,
                LoginDate = DateTime.Now,
                LastActivityDate = DateTime.Now,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<List<UserSession>> GetActiveSessionsAsync(int userId)
        {
            return await _context.UserSessions
                .Where(s => s.ApplicationUserId == userId && s.IsActive)
                .OrderByDescending(s => s.LastActivityDate)
                .ToListAsync();
        }

        public async Task TouchSessionAsync(int userId, string sessionToken)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId
                    && s.SessionToken == sessionToken && s.IsActive);

            if (session != null)
            {
                session.LastActivityDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> RevokeAllOtherSessionsAsync(int userId, string currentSessionToken)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.ApplicationUserId == userId
                    && s.IsActive
                    && s.SessionToken != currentSessionToken)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return sessions.Count;
        }

        public async Task RevokeSessionAsync(int userId, string sessionToken)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.ApplicationUserId == userId
                    && s.SessionToken == sessionToken && s.IsActive);

            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        private static (string deviceInfo, string operatingSystem) ParseUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return ("نامشخص", "نامشخص");

            var ua = userAgent;

            // Parse OS
            string os = "نامشخص";
            if (ua.Contains("Windows NT 10")) os = "Windows 10/11";
            else if (ua.Contains("Windows NT 6.3")) os = "Windows 8.1";
            else if (ua.Contains("Windows NT 6.1")) os = "Windows 7";
            else if (ua.Contains("Windows")) os = "Windows";
            else if (ua.Contains("Mac OS X")) os = "macOS";
            else if (ua.Contains("Linux")) os = "Linux";
            else if (ua.Contains("Android")) os = "Android";
            else if (ua.Contains("iPhone") || ua.Contains("iPad")) os = "iOS";

            // Parse Browser
            string device = "مرورگر نامشخص";
            if (ua.Contains("Edg/"))
                device = "Microsoft Edge";
            else if (ua.Contains("OPR/") || ua.Contains("Opera"))
                device = "Opera";
            else if (ua.Contains("Chrome/") && !ua.Contains("Edg/"))
                device = "Google Chrome";
            else if (ua.Contains("Firefox/"))
                device = "Mozilla Firefox";
            else if (ua.Contains("Safari/") && ua.Contains("Version/"))
                device = "Safari";

            // Parse Mobile
            if (ua.Contains("Mobile") || ua.Contains("Android"))
                device += " (موبایل)";

            return (device, os);
        }
    }
}
