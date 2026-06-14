using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prototype.Data;
using Prototype.Models;
using Prototype.ViewModels;
using System;
 
 namespace Prototype.Controllers
{
    [Authorize]
    public class OrdersController : Controller 
    {
        private readonly AppDbContext db;

        public OrdersController(AppDbContext db) 
        {
            this.db = db;
        }

        public IActionResult Index() => View(db.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.CreatedDate)
            .ToList());

        [Authorize(Roles = "Employee,Manager")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Products = db.Products.OrderBy(x => x.Name).ToList();
            return View(new CreateOrderViewModel());
        }

        [Authorize(Roles = "Employee,Manager")]
        [HttpPost]
        public IActionResult Create(CreateOrderViewModel model)
        {
            ViewBag.Products = db.Products.OrderBy(x => x.Name).ToList();
            if (!ModelState.IsValid) return View(model);

            var product = db.Products.SingleOrDefault(x => x.Id == model.ProductId); 
            if (product is null)
            {
                ModelState.AddModelError(string.Empty, "Товар не найден");
                return View(model);
            }

            if (model.Quantity > product.QuantityInStock)
            {
                ModelState.AddModelError(nameof(model.Quantity), $"Недостаточно товара на складе. Доступно: {product.QuantityInStock}");
                return View(model);
            }

            if (db.Orders.Any(x => x.OrderNumber == model.OrderNumber))
            {
                ModelState.AddModelError(nameof(model.OrderNumber), "Заказ с таким номером уже существует");
                return View(model);
            }

            var order = new Order
            {
                OrderNumber = model.OrderNumber.Trim(),
                Status = OrderStatus.Created,
                Items = new List<OrderItem> { new OrderItem { ProductId = model.ProductId, Quantity = model.Quantity } } 
            };

            db.Orders.Add(order);
            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Employee,Manager")]
        public IActionResult ToAssembly(int id)
        {
            var order = db.Orders.Find(id);
            if (order is null) return NotFound();
            if (order.Status != OrderStatus.Created)
            {
                TempData["Error"] = "В сборку можно перевести только заказ в статусе 'Создан'.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = OrderStatus.InAssembly;
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Employee,Manager")]
        public IActionResult ConfirmAssembly(int id)
        {
            var order = db.Orders.Include(x => x.Items).FirstOrDefault(x => x.Id == id);
            if (order is null) return NotFound();
            if (order.Status != OrderStatus.InAssembly)
            {
                TempData["Error"] = "Подтвердить комплектацию можно только для заказа в сборке.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in order.Items)
            {
                var product = db.Products.Find(item.ProductId);
                if (product is null || product.QuantityInStock < item.Quantity)
                {
                    TempData["Error"] = $"Недостаточно остатков для товара ID={item.ProductId}";
                    return RedirectToAction(nameof(Index));
                }
            }

            foreach (var item in order.Items)
            {
                var product = db.Products.Find(item.ProductId)!;
                product.QuantityInStock -= item.Quantity;
                db.StockOperations.Add(new StockOperation
                {
                    ProductId = item.ProductId,
                    OperationType = StockOperationType.WriteOff,
                    Quantity = item.Quantity,
                    Comment = $"Списание под заказ {order.OrderNumber}",
                    OperatorLogin = CurrentOperatorLogin()
                });
            }

            order.Status = OrderStatus.ReadyForPickup;
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Employee,Manager")]
        [HttpGet]
        public IActionResult Issue(int id)
        {
            var order = db.Orders.Find(id);
            if (order is null) return NotFound();
            if (order.Status != OrderStatus.ReadyForPickup)
            {
                TempData["Error"] = "Выдать можно только заказ, готовый к получению.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [Authorize(Roles = "Employee,Manager")]
        [HttpPost]
        public IActionResult ConfirmIssue(int id, string recipientDocument)
        {
            var order = db.Orders
                .Include(x => x.Items)
                .FirstOrDefault(x => x.Id == id);

            if (order is null) return NotFound();
            if (order.Status != OrderStatus.ReadyForPickup)
            {
                TempData["Error"] = "Выдать можно только заказ, готовый к получению.";
                return RedirectToAction(nameof(Index));
            }

            var document = recipientDocument?.Trim();
            if (string.IsNullOrWhiteSpace(document))
            {
                ModelState.AddModelError(nameof(recipientDocument), "Укажите документ получателя.");
                return View("Issue", order);
            }

            order.RecipientDocument = document;
            order.Status = OrderStatus.Issued;
            order.IssuedDate = DateTime.UtcNow;

            foreach (var item in order.Items)
            {
                db.StockOperations.Add(new StockOperation
                {
                    ProductId = item.ProductId,
                    OperationType = StockOperationType.Issue,
                    Quantity = item.Quantity,
                    Comment = $"Выдача заказа {order.OrderNumber}",
                    OperatorLogin = CurrentOperatorLogin()
                });
            }

            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private string CurrentOperatorLogin() => User?.Identity?.Name ?? "system";
    }

}
