using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Account;
using SmartTask.Web.Services.PasswordHasher;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace SmartTask.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasherService _passwordHasher;

        public AccountController(
            IUnitOfWork unitOfWork,
            IPasswordHasherService passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = _passwordHasher.HashPassword(model.Password),

                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _unitOfWork.ApplicationUsers.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = (await _unitOfWork.ApplicationUsers
                .FindAsync(x => x.Email == model.Email))
                .FirstOrDefault();

            if (user == null)
            {
                ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");
                return View(model);
            }

            var result = _passwordHasher.VerifyPassword(
                model.Password,
                user.PasswordHash);

            if (!result)
            {
                ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است.");
                return View(model);
            }

            user.LastLoginDate = DateTime.Now;

            _unitOfWork.ApplicationUsers.Update(user);

            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("FirstName",user.FirstName),
                new Claim("LastName",user.LastName)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}