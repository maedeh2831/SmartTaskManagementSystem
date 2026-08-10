using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Settings;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(ISettingsService settingsService, UserManager<ApplicationUser> userManager)
        {
            _settingsService = settingsService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var model = await _settingsService.GetSettingsAsync(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(AccountSettingsViewModel model)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateAccountAsync(userId, model);
            TempData["Success"] = "تنظیمات حساب با موفقیت ذخیره شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppearance(AppearanceSettingsViewModel model)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateAppearanceAsync(userId, model);
            TempData["Success"] = "تنظیمات ظاهری با موفقیت ذخیره شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotifications(List<NotificationPreferenceItemViewModel> model)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateNotificationsAsync(userId, model);
            TempData["Success"] = "تنظیمات اعلان‌ها با موفقیت ذخیره شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDefaultWorkspace(int? workspaceId)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateDefaultWorkspaceAsync(userId, workspaceId);
            TempData["Success"] = "Workspace پیش‌فرض به‌روزرسانی شد.";
            return RedirectToAction(nameof(Index));
        }
    }
}