using Microsoft.AspNetCore.Identity;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using System.Security.Claims;

namespace SmartTask.Web.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        private ApplicationUser? _user;

        public ApplicationUser? CurrentUser
        {
            get
            {
                if (_user != null)
                    return _user;

                if (!IsAuthenticated)
                    return null;

                _user = _userManager
                    .GetUserAsync(_httpContextAccessor.HttpContext!.User)
                    .Result;

                return _user;
            }
        }

        private ClaimsPrincipal ClaimsUser
            => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

        public bool IsAuthenticated
            => ClaimsUser.Identity?.IsAuthenticated ?? false;

        public int UserId
        {
            get
            {
                var value = ClaimsUser.FindFirstValue(ClaimTypes.NameIdentifier);

                return int.TryParse(value, out var id)
                    ? id
                    : 0;
            }
        }

        public string Email
            => CurrentUser?.Email ?? "";

        public string FullName
            => CurrentUser == null
                ? ""
                : $"{CurrentUser.FirstName} {CurrentUser.LastName}";

        public bool IsAdmin
            => ClaimsUser.IsInRole("Admin");

        public string? Avatar => _user?.Avatar;
    }
}
