using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class GamificationMVCController : BaseController
{
    public GamificationMVCController(ICurrentUserService currentUser)
        : base(currentUser) { }

    public IActionResult Marketplace() => View("~/Views/Gamification/Marketplace.cshtml");

    public IActionResult Leaderboards() => View("~/Views/Gamification/Leaderboards.cshtml");

    public IActionResult Inventory() => View("~/Views/Gamification/Inventory.cshtml");

    public IActionResult Achievements() => View("~/Views/Gamification/Achievements.cshtml");

    public IActionResult Milestones() => View("~/Views/Gamification/Milestones.cshtml");

    public IActionResult ProfileDashboard() => View("~/Views/Gamification/ProfileDashboard.cshtml");
}
