using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prototype.Controllers;
using Prototype.Data;
using Prototype.Models;
using Prototype.ViewModels;

namespace Prototype.Tests;

public class WarehouseControllerTests
{
    [Fact]
    public void Receipt_SavesOperatorLoginInStockOperation()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AppDbContext(options);
        var product = new Product
        {
            Name = "Товар для приемки",
            Description = "Проверка оператора",
            QuantityInStock = 4
        };
        db.Products.Add(product);
        db.SaveChanges();

        var controller = new WarehouseController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, "warehouse-operator")
                    ], "TestAuth"))
                }
            }
        };

        var result = controller.Receipt(new ReceiptViewModel
        {
            ProductId = product.Id,
            Quantity = 6,
            Date = new DateTime(2026, 6, 15)
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(10, db.Products.Single().QuantityInStock);
        var operation = db.StockOperations.Single();
        Assert.Equal(StockOperationType.Receipt, operation.OperationType);
        Assert.Equal("warehouse-operator", operation.OperatorLogin);
    }
}
