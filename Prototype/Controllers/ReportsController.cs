using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prototype.Data;
using Prototype.Models;
using System.Text;

namespace Prototype.Controllers
{
    [Authorize(Roles = "Manager,Lead")]
    public class ReportsController(AppDbContext db) : Controller
    {
        public IActionResult Index()
        {
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.IssuedOrders = db.Orders.Count(x => x.Status == OrderStatus.Issued);
            ViewBag.ReadyOrders = db.Orders.Count(x => x.Status == OrderStatus.ReadyForPickup);
            ViewBag.StockOperations = db.StockOperations.Include(x => x.Product).OrderByDescending(x => x.Date).Take(50).ToList();
            return View();
        }

        public IActionResult IssuedOrders() => View(db.Orders
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Where(x => x.Status == OrderStatus.Issued)
            .OrderByDescending(x => x.CreatedDate)
            .ToList());

        public IActionResult IssuedOrdersCsv()
        {
            var orders = db.Orders
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .Where(x => x.Status == OrderStatus.Issued)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("OrderNumber,CreatedDate,RecipientDocument,Items");

            foreach (var order in orders)
            {
                var items = string.Join("; ", order.Items.Select(i => $"{i.Product?.Name} x{i.Quantity}"));
                sb.AppendLine($"{order.OrderNumber},{order.CreatedDate:O},{order.RecipientDocument},{items}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "issued-orders.csv");
        }
    }
}
