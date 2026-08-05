using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(
            ILogger<HomeController> logger,
            IUserDashboardService userDashboardService,
            ICurrentUserService currentUser)
            : base(currentUser)
        {
            _logger = logger;
            _userDashboardService = userDashboardService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
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