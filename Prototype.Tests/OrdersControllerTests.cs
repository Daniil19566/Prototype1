using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Prototype.Controllers;
using Prototype.Data;
using Prototype.Models;
using Prototype.ViewModels;
using System.Security.Claims;

namespace Prototype.Tests;

public class OrdersControllerTests
{
    [Fact]
    public void OrderLifecycle_FromCreateToIssue_UpdatesOrderStockAndOperations()
    {
        using var db = CreateDbContext();
        var product = new Product
        {
            Name = "Тестовый товар",
            Description = "Товар для проверки жизненного цикла заказа",
            QuantityInStock = 10
        };

        db.Products.Add(product);
        db.SaveChanges();

        var controller = CreateController(db, "order-operator");
        var model = new CreateOrderViewModel
        {
            OrderNumber = "PVZ-TEST-1",
            ProductId = product.Id,
            Quantity = 3
        };

        var createResult = controller.Create(model);

        Assert.IsType<RedirectToActionResult>(createResult);
        var order = db.Orders.Include(x => x.Items).Single();
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Null(order.IssuedDate);

        var toAssemblyResult = controller.ToAssembly(order.Id);

        Assert.IsType<RedirectToActionResult>(toAssemblyResult);
        Assert.Equal(OrderStatus.InAssembly, db.Orders.Single().Status);

        var confirmAssemblyResult = controller.ConfirmAssembly(order.Id);

        Assert.IsType<RedirectToActionResult>(confirmAssemblyResult);
        Assert.Equal(OrderStatus.ReadyForPickup, db.Orders.Single().Status);
        Assert.Null(db.Orders.Single().IssuedDate);
        Assert.Equal(7, db.Products.Single().QuantityInStock);
        Assert.Contains(db.StockOperations, x =>
            x.OperationType == StockOperationType.WriteOff
            && x.ProductId == product.Id
            && x.Quantity == 3
            && x.OperatorLogin == "order-operator");

        var issueResult = controller.Issue(order.Id);

        Assert.IsType<ViewResult>(issueResult);

        var confirmIssueResult = controller.ConfirmIssue(order.Id, "Паспорт 1234");

        Assert.IsType<RedirectToActionResult>(confirmIssueResult);
        var issuedOrder = db.Orders.Single();
        Assert.Equal(OrderStatus.Issued, issuedOrder.Status);
        Assert.Equal("Паспорт 1234", issuedOrder.RecipientDocument);
        Assert.NotNull(issuedOrder.IssuedDate);
        Assert.Contains(db.StockOperations, x =>
            x.OperationType == StockOperationType.Issue
            && x.ProductId == product.Id
            && x.Quantity == 3
            && x.OperatorLogin == "order-operator");
    }

    [Fact]
    public void Create_WhenQuantityExceedsStock_DoesNotCreateOrderAndAddsModelError()
    {
        using var db = CreateDbContext();
        var product = new Product
        {
            Name = "Товар с малым остатком",
            Description = "Проверка ограничения остатка",
            QuantityInStock = 2
        };

        db.Products.Add(product);
        db.SaveChanges();

        var controller = new OrdersController(db);
        var model = new CreateOrderViewModel
        {
            OrderNumber = "PVZ-LOW-STOCK",
            ProductId = product.Id,
            Quantity = 5
        };

        var result = controller.Create(model);

        Assert.IsType<ViewResult>(result);
        Assert.Empty(db.Orders);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(CreateOrderViewModel.Quantity)));
    }

    [Fact]
    public void ConfirmIssue_WithoutRecipientDocument_KeepsOrderReadyAndAddsModelError()
    {
        using var db = CreateDbContext();
        var product = new Product
        {
            Name = "Товар к выдаче",
            Description = "Проверка запрета выдачи без документа",
            QuantityInStock = 5
        };
        var order = new Order
        {
            OrderNumber = "PVZ-NO-DOC",
            Status = OrderStatus.ReadyForPickup,
            Items =
            [
                new OrderItem
                {
                    Product = product,
                    Quantity = 2
                }
            ]
        };

        db.Orders.Add(order);
        db.SaveChanges();

        var controller = new OrdersController(db);

        var result = controller.ConfirmIssue(order.Id, string.Empty);

        Assert.IsType<ViewResult>(result);
        var unchangedOrder = db.Orders.Single();
        Assert.Equal(OrderStatus.ReadyForPickup, unchangedOrder.Status);
        Assert.True(string.IsNullOrWhiteSpace(unchangedOrder.RecipientDocument));
        Assert.Null(unchangedOrder.IssuedDate);
        Assert.DoesNotContain(db.StockOperations, x => x.OperationType == StockOperationType.Issue);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("recipientDocument"));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static OrdersController CreateController(AppDbContext db, string login)
    {
        return new OrdersController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, login)
                    ], "TestAuth"))
                }
            }
        };
    }
}
