using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prototype.Controllers;
using Prototype.Data;
using Prototype.Models;

namespace Prototype.Tests;

public class ReportsControllerTests
{
    [Fact]
    public void IssuedOrdersCsv_EscapesTextFields()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AppDbContext(options);
        var product = new Product
        {
            Name = "Товар \"A\",\r\nбольшой",
            Description = "CSV escaping product",
            QuantityInStock = 1
        };
        var orderWithDocument = new Order
        {
            OrderNumber = "PVZ,\"1\"",
            Status = OrderStatus.Issued,
            CreatedDate = new DateTime(2026, 6, 14, 10, 30, 0, DateTimeKind.Utc),
            IssuedDate = new DateTime(2026, 6, 14, 12, 0, 0, DateTimeKind.Utc),
            RecipientDocument = "Паспорт \"1234\", серия\nAB",
            Items =
            [
                new OrderItem
                {
                    Product = product,
                    Quantity = 2
                }
            ]
        };
        var orderWithoutDocument = new Order
        {
            OrderNumber = "PVZ-EMPTY-DOC",
            Status = OrderStatus.Issued,
            CreatedDate = new DateTime(2026, 6, 14, 11, 0, 0, DateTimeKind.Utc),
            RecipientDocument = null
        };

        db.Orders.AddRange(orderWithDocument, orderWithoutDocument);
        db.SaveChanges();

        var controller = new ReportsController(db);

        var result = Assert.IsType<FileContentResult>(controller.IssuedOrdersCsv());
        var csv = Encoding.UTF8.GetString(result.FileContents);

        Assert.Contains("\"PVZ,\"\"1\"\"\"", csv);
        Assert.Contains("OrderNumber,CreatedDate,IssuedDate,RecipientDocument,Items", csv);
        Assert.Contains("\"Паспорт \"\"1234\"\", серия\nAB\"", csv);
        Assert.Contains("\"Товар \"\"A\"\",\r\nбольшой x2\"", csv);
        Assert.Contains("PVZ-EMPTY-DOC,2026-06-14T11:00:00.0000000Z,,,", csv);
        Assert.Contains("2026-06-14T12:00:00.0000000Z", csv);
    }
}
