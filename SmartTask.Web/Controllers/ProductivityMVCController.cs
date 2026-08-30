using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ProductivityMVCController : BaseController
{
    public ProductivityMVCController(ICurrentUserService currentUser)
        : base(currentUser) { }

    public IActionResult Index() => View("~/Views/Gamification/ProductivityDashboard.cshtml");
}
