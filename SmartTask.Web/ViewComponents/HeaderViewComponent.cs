using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Shared;
using SmartTask.Web.Services.Gamification;

namespace SmartTask.Web.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IEquippedCosmeticsService _cosmeticsService;

        public HeaderViewComponent(
            ICurrentUserService currentUser,
            IEquippedCosmeticsService cosmeticsService)
        {
            _currentUser = currentUser;
            _cosmeticsService = cosmeticsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new HeaderViewModel
            {
                FullName = _currentUser.FullName,
                Email = _currentUser.Email,
                IsAdmin = _currentUser.IsAdmin,
                Avatar = _currentUser.Avatar
            };

            if (_currentUser.IsAuthenticated)
                model.Cosmetics = await _cosmeticsService.GetForUserAsync(_currentUser.UserId);

            return View(model);
        }
    }
}
