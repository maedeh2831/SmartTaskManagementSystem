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
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserSessionTracker _sessionTracker;

        public SettingsController(
            ISettingsService settingsService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserSessionTracker sessionTracker)
        {
            _settingsService = settingsService;
            _userManager = userManager;
            _signInManager = signInManager;
            _sessionTracker = sessionTracker;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var model = await _settingsService.GetSettingsAsync(userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccount(AccountSettingsViewModel Account)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateAccountAsync(userId, Account);
            TempData["Success"] = "تنظیمات حساب با موفقیت ذخیره شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppearance(AppearanceSettingsViewModel Appearance)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateAppearanceAsync(userId, Appearance);
            TempData["Success"] = "تنظیمات ظاهری با موفقیت ذخیره شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotifications(List<NotificationPreferenceItemViewModel> Notifications)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateNotificationsAsync(userId, Notifications);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateManagement(ManagementSettingsViewModel Management)
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            await _settingsService.UpdateManagementAsync(userId, Management);
            TempData["Success"] = "تنظیمات مدیریتی با موفقیت ذخیره شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveSessions()
        {
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var sessions = await _sessionTracker.GetActiveSessionsAsync(userId);

            var currentToken = HttpContext.Session.GetString("UserSessionToken");

            var result = sessions.Select(s => new
            {
                id = s.Id,
                deviceInfo = s.DeviceInfo ?? "نامشخص",
                operatingSystem = s.OperatingSystem ?? "نامشخص",
                ipAddress = s.IpAddress ?? "نامشخص",
                loginDate = s.LoginDate.ToString("yyyy/MM/dd HH:mm"),
                lastActivity = s.LastActivityDate.ToString("yyyy/MM/dd HH:mm"),
                isCurrent = s.SessionToken == currentToken
            }).ToList();

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutAllDevices()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            // Update SecurityStamp — this invalidates all existing auth cookies
            var stampResult = await _userManager.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded)
            {
                return Json(new { success = false, message = "خطا در خروج از دستگاه‌ها." });
            }

            // Also revoke all tracked sessions in DB
            var currentToken = HttpContext.Session.GetString("UserSessionToken");
            if (!string.IsNullOrEmpty(currentToken))
            {
                await _sessionTracker.RevokeAllOtherSessionsAsync(user.Id, currentToken);
            }

            // Sign out and re-sign in so current user keeps their session
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, false);

            // Re-track current session
            var ua = Request.Headers.UserAgent.ToString();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var newSession = await _sessionTracker.TrackLoginAsync(user.Id, ua, ip, HttpContext);
            HttpContext.Session.SetString("UserSessionToken", newSession.SessionToken);

            return Json(new { success = true, message = "سایر دستگاه‌ها با موفقیت خارج شدند." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            // Soft-delete using the project's existing ViewState pattern
            user.ViewState = false;
            user.IsActive = false;
            user.ChangeDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Invalidate all sessions by updating SecurityStamp
            await _userManager.UpdateSecurityStampAsync(user);

            // Sign out
            await _signInManager.SignOutAsync();

            return Json(new { success = true, message = "حساب کاربری با موفقیت حذف شد." });
        }
    }
}