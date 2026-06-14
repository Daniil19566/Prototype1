using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prototype.Data;
using Prototype.Models;
using Prototype.Services;
using System.Security.Claims;

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

            var user = new User
            {
                Login = login.Trim(),
                Role = role
            };

            user.PasswordHash = hasher.Hash(user, password);
            db.Users.Add(user);

            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = db.Users.Find(id);
            if (user is null)
            {
                TempData["Error"] = "Пользователь не найден";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserId, out var currentId) && currentId == id)
            {
                TempData["Error"] = "Нельзя удалить текущего пользователя";
                return RedirectToAction(nameof(Index));
            }

            if ((user.Role == UserRole.Manager || user.Role == UserRole.SysAdmin)
                && db.Users.Count(x => x.Role == UserRole.Manager || x.Role == UserRole.SysAdmin) <= 1)
            {
                TempData["Error"] = "Нельзя удалить последнего администратора или управляющего";
                return RedirectToAction(nameof(Index));
            }

            db.Users.Remove(user);
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

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = db.Products.Find(id);
            if (product is null)
            {
                TempData["Error"] = "Товар не найден";
                return RedirectToAction(nameof(Index));
            }

            if (db.OrderItems.Any(x => x.ProductId == id))
            {
                TempData["Error"] = "Нельзя удалить товар, который используется в заказах";
                return RedirectToAction(nameof(Index));
            }

            if (db.StockOperations.Any(x => x.ProductId == id))
            {
                TempData["Error"] = "Нельзя удалить товар, по которому есть складские операции";
                return RedirectToAction(nameof(Index));
            }

            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
