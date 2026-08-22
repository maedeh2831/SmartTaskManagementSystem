namespace SmartTask.Web.Models.Entities
{
    public class UserSession
    {
        public int Id { get; set; }
        public int ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        /// <summary>
        /// Unique identifier for this session, stored in the auth cookie.
        /// </summary>
        public string SessionToken { get; set; } = null!;

        /// <summary>
        /// Browser/device name parsed from User-Agent.
        /// </summary>
        public string? DeviceInfo { get; set; }

        /// <summary>
        /// Operating system parsed from User-Agent.
        /// </summary>
        public string? OperatingSystem { get; set; }

        /// <summary>
        /// IP address of the login.
        /// </summary>
        public string? IpAddress { get; set; }

        public DateTime LoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }

        /// <summary>
        /// False means this session has been revoked (logout all).
        /// </summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
