using System.Security.Claims;

namespace SmartTask.Web.Common.Helpers
{
    public static class ClaimHelper
    {
        public const string UserId = ClaimTypes.NameIdentifier;
        public const string Email = ClaimTypes.Email;
        public const string UserName = ClaimTypes.Name;
        public const string Role = ClaimTypes.Role;
    }
}