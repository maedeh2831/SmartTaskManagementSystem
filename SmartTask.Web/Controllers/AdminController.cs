using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : BaseController
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminController(IAdminDashboardService adminDashboardService, ICurrentUserService currentUser)
        : base(currentUser)
    {
        _adminDashboardService = adminDashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _adminDashboardService.GetDashboardAsync();
        return View(model);
    }
}