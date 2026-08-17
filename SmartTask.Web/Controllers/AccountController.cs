using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Account;
using SmartTask.Web.Services.Email;
using SmartTask.Web.Services.Files;
using SmartTask.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartTask.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IWorkspaceInvitationService _invitationService;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileUploadService _fileUploadService;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IWorkspaceInvitationService invitationService,
            ICurrentUserService currentUser,
            IFileUploadService fileUploadService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _invitationService = invitationService;
            _currentUser = currentUser;
            _fileUploadService = fileUploadService;
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            await _emailService.SendEmailAsync(
                "maedeheslami2831@gmail.com",
                "SmartTask Test",
                "<h2>سلام 👋</h2><p>اولین ایمیل SmartTask با موفقیت ارسال شد.</p>");

            return Content("Sent Email Successfully");
        }

        [HttpGet]
        public async Task<IActionResult> Register(Guid? invitationToken)
        {
            var model = new RegisterViewModel();

            if (invitationToken.HasValue)
            {
                var invitation = await _invitationService.GetByTokenAsync(invitationToken.Value);

                if (invitation != null &&
                    invitation.Status == SmartTask.Web.Models.Enums.WorkspaceInvitationStatusType.Pending &&
                    invitation.ExpiryDate >= DateTime.Now)
                {
                    model.Email = invitation.Email;
                    model.InvitationToken = invitationToken;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            Console.WriteLine("REGISTER POST CALLED");
            if (!ModelState.IsValid)
                return View(model);

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "این ایمیل قبلاً ثبت شده است."
                );
                return View(model);
            }

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email,
                IsActive = true,
                TimeZone = "Asia/Tehran"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // create confirm email token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // create confirm link
                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new
                    {
                        userId = user.Id,
                        token = token,
                        invitationToken = model.InvitationToken
                    },
                    Request.Scheme);

                // send email
                await _emailService.SendEmailAsync(
                    user.Email!,
                    "تایید ایمیل SmartTask",
                    $@"
                        <h2>به SmartTask خوش آمدید 👋</h2>

                        <p>
                            برای فعال‌سازی حساب روی دکمه زیر کلیک کنید.
                        </p>

                        <p>
                            <a href='{confirmationLink}'
                               style='background:#4F46E5;
                                      color:white;
                                      padding:12px 25px;
                                      text-decoration:none;
                                      border-radius:8px'>
                                تایید حساب
                            </a>
                        </p>

                        <p>
                            اگر این درخواست متعلق به شما نیست، این ایمیل را نادیده بگیرید.
                        </p>
                        ");

                TempData["Success"] =
                    "لینک تایید ایمیل برای شما ارسال شد.";

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(
                    "", "حساب کاربری شما هنوز فعال نشده است. لطفاً ابتدا ایمیل خود را تایید کنید.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                user.LastLoginDate = DateTime.Now;
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(
                nameof(ExternalLoginCallback),
                "Account",
                new { returnUrl });

            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(
                    provider,
                    redirectUrl);

            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(
            string? returnUrl = null,
            string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                TempData["Error"] = "ورود با حساب گوگل انجام نشد.";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                TempData["Error"] = "دریافت اطلاعات حساب گوگل با مشکل مواجه شد.";
                return RedirectToAction(nameof(Login));
            }

            // اگر قبلاً با گوگل ثبت شده باشد
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (result.Succeeded)
            {
                TempData["Success"] = "با موفقیت وارد شدید.";

                return LocalRedirect(returnUrl);
            }

            // گرفتن ایمیل از گوگل
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "ایمیل از گوگل دریافت نشد.";

                return RedirectToAction(nameof(Login));
            }

            // اگر قبلاً کاربر با این ایمیل وجود دارد
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var firstName =
                    info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";

                var lastName =
                    info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };

                var createResult =
                    await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "حساب کاربری ایجاد نشد. دوباره تلاش کنید.";

                    return RedirectToAction(nameof(Login));
                }

                await _userManager.AddToRoleAsync(user, "Member");
            }

            // اگر قبلاً این Login متصل نشده باشد
            var existingLogins = await _userManager.GetLoginsAsync(user);

            if (!existingLogins.Any(x =>
                    x.LoginProvider == info.LoginProvider &&
                    x.ProviderKey == info.ProviderKey))
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);

                if (!addLoginResult.Succeeded)
                {
                    TempData["Error"] = "خطا در اتصال حساب گوگل.";

                    return RedirectToAction(nameof(Login));
                }
            }

            await _signInManager.SignInAsync(user, false);

            TempData["Success"] = "ورود با گوگل انجام شد.";

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "کاربری با این ایمیل یافت نشد.");
                return View(model);
            }

            // Create Token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create Link
            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new
                {
                    token = token,
                    email = user.Email
                },
                Request.Scheme);

            // Send Email
            await _emailService.SendEmailAsync(
                user.Email!,
                "بازیابی رمز عبور SmartTask",
                $@"
                    <h2>بازیابی رمز عبور</h2>

                    <p>برای تغییر رمز عبور روی دکمه زیر کلیک کنید.</p>

                    <p>
                        <a href='{resetLink}'
                           style='
                             background:#4F46E5;
                             color:white;
                             padding:12px 25px;
                             border-radius:8px;
                             text-decoration:none;'>
                             تغییر رمز عبور
                        </a>
                    </p>

                    <p>اگر این درخواست از طرف شما نبوده، این ایمیل را نادیده بگیرید.</p>
                    ");

            TempData["Success"] =
                "لینک بازیابی رمز عبور برای شما ارسال شد.";

            return RedirectToAction(nameof(Login));

        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return RedirectToAction(nameof(Login));

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "کاربر پیدا نشد.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "رمز عبور با موفقیت تغییر یافت.";

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId, string token, Guid? invitationToken)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(Login));

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                if (invitationToken.HasValue)
                {
                    try
                    {
                        await _invitationService.AcceptInvitationAfterRegisterAsync(
                            invitationToken.Value, user.Id);

                        TempData["Success"] =
                            "ایمیل شما تایید شد و به فضای کاری دعوت‌شده اضافه شدید.";
                    }
                    catch
                    {
                        TempData["Success"] = "ایمیل شما با موفقیت تایید شد.";
                    }
                }
                else
                {
                    TempData["Success"] = "ایمیل شما با موفقیت تایید شد.";
                }

                return RedirectToAction(nameof(Login));
            }

            TempData["Error"] = "لینک تایید نامعتبر یا منقضی شده است.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                JobTitle = user.JobTitle,
                Bio = user.Bio,
                Avatar = user.Avatar
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? "";
                model.Avatar = user.Avatar;

                return View(model);
            }

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.JobTitle = string.IsNullOrWhiteSpace(model.JobTitle)
                ? null
                : model.JobTitle.Trim();
            user.Bio = string.IsNullOrWhiteSpace(model.Bio)
                ? null
                : model.Bio.Trim();

            if (model.NewAvatar != null)
            {
                var allowedExtensions = new[]
                {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

                var extension =
                    Path.GetExtension(model.NewAvatar.FileName)
                        .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(model.NewAvatar),
                        "فرمت تصویر مجاز نیست.");

                    model.Email = user.Email ?? "";
                    model.Avatar = user.Avatar;

                    return View(model);
                }

                if (model.NewAvatar.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(model.NewAvatar),
                        "حجم تصویر نباید بیشتر از 2 مگابایت باشد.");

                    model.Email = user.Email ?? "";
                    model.Avatar = user.Avatar;

                    return View(model);
                }

                var oldAvatar = user.Avatar;

                user.Avatar = await _fileUploadService.SaveFileAsync(
                    model.NewAvatar,
                    "avatars");

                if (!string.IsNullOrWhiteSpace(oldAvatar))
                {
                    _fileUploadService.DeleteFile(oldAvatar);
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                model.Email = user.Email ?? "";
                model.Avatar = user.Avatar;

                return View(model);
            }

            TempData["Success"] = "پروفایل شما با موفقیت بروزرسانی شد.";

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAvatar()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                _fileUploadService.DeleteFile(user.Avatar);

                user.Avatar = null;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    TempData["Error"] =
                        "حذف تصویر پروفایل انجام نشد.";

                    return RedirectToAction(nameof(Profile));
                }
            }

            TempData["Success"] =
                "تصویر پروفایل حذف شد.";

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword
            );

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "رمز عبور شما با موفقیت تغییر کرد.";

                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "PasswordMismatch")
                {
                    ModelState.AddModelError(
                        nameof(model.CurrentPassword),
                        "رمز عبور فعلی صحیح نیست."
                    );
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }
            }

            return View(model);
        }


        /// <summary>
        /// ثبت شناسه مشترک Webpushr (SID) برای کاربر جاری تا بتوان
        /// اعلان‌های Push پیام چت را برای او ارسال کرد.
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWebpushrSubscriber(
            long sid)
        {
            if (sid <= 0)
            {
                return BadRequest(
                    new { success = false, message = "SID نامعتبر است." });
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            user.WebpushrSubscriberId = sid;

            var result = await _userManager.UpdateAsync(user);

            return Json(new { success = result.Succeeded });
        }


    }
}