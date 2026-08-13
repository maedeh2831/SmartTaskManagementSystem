using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Shared;

namespace SmartTask.Web.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly ICurrentUserService _currentUser;

        public HeaderViewComponent(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        public IViewComponentResult Invoke()
        {
            var model = new HeaderViewModel
            {
                FullName = _currentUser.FullName,
                Email = _currentUser.Email,
                IsAdmin = _currentUser.IsAdmin,
                Avatar = _currentUser.Avatar
            };

            return View(model);
        }
    }
}