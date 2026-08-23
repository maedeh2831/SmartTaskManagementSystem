using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IUserSessionTracker
    {
        /// <summary>
        /// Record a new login session for the user.
        /// </summary>
        Task<UserSession> TrackLoginAsync(int userId, string userAgent, string? ipAddress, HttpContext httpContext);

        /// <summary>
        /// Get all active sessions for a user.
        /// </summary>
        Task<List<UserSession>> GetActiveSessionsAsync(int userId);

        /// <summary>
        /// Update the last activity timestamp for a session.
        /// </summary>
        Task TouchSessionAsync(int userId, string sessionToken);

        /// <summary>
        /// Revoke all sessions except the current one (logout all other devices).
        /// Returns the number of revoked sessions.
        /// </summary>
        Task<int> RevokeAllOtherSessionsAsync(int userId, string currentSessionToken);

        /// <summary>
        /// Revoke a specific session by token.
        /// </summary>
        Task RevokeSessionAsync(int userId, string sessionToken);
    }
}
