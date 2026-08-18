namespace SmartTask.Web.Models.ViewModels.Shared
{
    public class HeaderViewModel
    {
        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public bool IsAdmin { get; set; }

        public string? Avatar { get; set; }

        public string AvatarUrl => string.IsNullOrWhiteSpace(Avatar) ? "/images/default-avatar.svg" : Avatar;

    }
}