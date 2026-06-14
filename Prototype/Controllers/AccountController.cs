using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Prototype.Data;
using Prototype.Services;
using Prototype.ViewModels;
using Prototype.Models;
using System;
using System.Security.Claims;


namespace Prototype.Controllers
{
    public class AccountController(AppDbContext db, PasswordHasherService hasher) : Controller
    {
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var hash = hasher.Hash(model.Password);
            var user = db.Users.FirstOrDefault(x => x.Login == model.Login && x.PasswordHash == hash);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
                return View(model);
            }

            var claims = new List<Claim>
        {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Login),
                new(ClaimTypes.Role, user.Role.ToString())
            };


            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return user.Role switch
            {
                UserRole.Lead => RedirectToAction("Index", "Reports"),
                UserRole.SysAdmin => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index", "Orders")
            };
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() => View();
    }
}
