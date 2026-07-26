using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Account;
using SmartTask.Web.Services.Email;

namespace SmartTask.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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
        public IActionResult Register()
        {
            return View();
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
                        token = token
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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");
                return View(model);
            }

            // جلوگیری از ورود قبل از تایید ایمیل
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(
                    "",
                    "حساب کاربری شما هنوز فعال نشده است. لطفاً ابتدا ایمیل خود را تایید کنید."
                );

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                false);


            if (result.Succeeded)
            {
                user.LastLoginDate = DateTime.Now;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("Index", "Home");
            }


            ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");

            return View(model);
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
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(Login));

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["Success"] = "ایمیل شما با موفقیت تایید شد.";

                return RedirectToAction(nameof(Login));
            }

            TempData["Error"] = "لینک تایید نامعتبر یا منقضی شده است.";

            return RedirectToAction(nameof(Login));
        }

    }
}