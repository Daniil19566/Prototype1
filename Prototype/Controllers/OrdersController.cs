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
                    Comment = $"Списание под заказ {order.OrderNumber}"
                });
            }

            order.Status = OrderStatus.ReadyForPickup;
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }

}
