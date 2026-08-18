using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Infrastructure.Interfaces
{
    public interface ICurrentUserService
    {
        ApplicationUser? CurrentUser { get; }

        int UserId { get; }

        string Email { get; }

        string FullName { get; }

        bool IsAuthenticated { get; }

        bool IsAdmin { get; }

        string? Avatar { get; }
    }
}