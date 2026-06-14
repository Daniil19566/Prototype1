using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prototype.Data;
using Prototype.Models;
using Prototype.Services;

namespace Prototype.Controllers
{
    [Authorize(Roles = "Manager,SysAdmin")]
    public class AdminController(AppDbContext db, PasswordHasherService hasher) : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Products = db.Products.OrderBy(x => x.Name).ToList();
            return View(db.Users.OrderBy(x => x.Login).ToList());
        }

        [HttpPost]
        public IActionResult CreateUser(string login, string password, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Логин и пароль обязательны";
                return RedirectToAction(nameof(Index));
            }
            if (db.Users.Any(x => x.Login == login.Trim()))
            {
                TempData["Error"] = "Пользователь с таким логином уже существует";
                return RedirectToAction(nameof(Index));
            }

            db.Users.Add(new User
            {
                Login = login.Trim(),
                PasswordHash = hasher.Hash(password),
                Role = role
            });

            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult CreateProduct(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Наименование товара обязательно";
                return RedirectToAction(nameof(Index));
            }

            db.Products.Add(new Product
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                QuantityInStock = 0
            });

            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
