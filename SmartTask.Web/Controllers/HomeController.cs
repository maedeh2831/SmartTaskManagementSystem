using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models;
using SmartTask.Web.Services.Interfaces;
using System.Diagnostics;

namespace SmartTask.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserDashboardService _userDashboardService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            IUserDashboardService userDashboardService,
            ICurrentUserService currentUser,
            ApplicationDbContext context)
            : base(currentUser)
        {
            _logger = logger;
            _userDashboardService = userDashboardService;
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var defaultWorkspaceId = CurrentUser.CurrentUser?.DefaultWorkspaceId;

            if (defaultWorkspaceId.HasValue)
            {
                // Validate the workspace still exists and user is still a member
                var isValid = await _context.Workspaces
                    .AnyAsync(w => w.Id == defaultWorkspaceId.Value
                                && w.ViewState
                                && w.Members.Any(m => m.ApplicationUserId == CurrentUser.UserId));

                if (isValid)
                    return RedirectToAction("Index", "WorkspaceDashboard", new { workspaceId = defaultWorkspaceId.Value });

                // Stale default — clear it and fall through to home dashboard
                var user = CurrentUser.CurrentUser;
                if (user != null)
                {
                    user.DefaultWorkspaceId = null;
                    await _context.SaveChangesAsync();
                }
            }

            var model = await _userDashboardService.GetDashboardAsync(CurrentUser.UserId);
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}