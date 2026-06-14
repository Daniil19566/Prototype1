using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prototype.Data;
using Prototype.Models;
using Prototype.ViewModels;
using System;


namespace Prototype.Controllers
{
    [Authorize(Roles = "Employee,Manager")]
    public class WarehouseController(AppDbContext db) : Controller
    {
        public IActionResult Index(string? search)
        {
            var query = db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }

            return View(query.OrderBy(x => x.Name).ToList());
        }

        [Authorize(Roles = "Employee,Manager")]
        [HttpGet]
        public IActionResult Receipt()
        {
            ViewBag.Products = db.Products.OrderBy(x => x.Name).ToList();
            return View(new ReceiptViewModel());
        }

        [Authorize(Roles = "Employee,Manager")]
        [HttpPost]
        public IActionResult Receipt(ReceiptViewModel model)
        {
            ViewBag.Products = db.Products.OrderBy(x => x.Name).ToList();
            if (!ModelState.IsValid) return View(model);

            var product = db.Products.FirstOrDefault(x => x.Id == model.ProductId);
            if (product is null)
            {
                ModelState.AddModelError(string.Empty, "Товар не найден");
                return View(model);
            }

            product.QuantityInStock += model.Quantity;
            db.StockOperations.Add(new StockOperation
            {
                ProductId = product.Id,
                OperationType = StockOperationType.Receipt,
                Quantity = model.Quantity,
                Date = model.Date,
                Comment = "Приём товара"
            });
            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Operations() => View(db.StockOperations
            .Include(x => x.Product)
            .OrderByDescending(x => x.Date)
            .Take(200)
            .ToList());
    }
}
