using Prototype.Models;

namespace Prototype.Tests;

public class DisplayHelpersTests
{
    [Theory]
    [InlineData(OrderStatus.Created, "Создан")]
    [InlineData(OrderStatus.InAssembly, "В сборке")]
    [InlineData(OrderStatus.ReadyForPickup, "Готов к выдаче")]
    [InlineData(OrderStatus.Issued, "Выдан")]
    public void OrderStatus_ToDisplayName_ReturnsRussianName(OrderStatus status, string expected)
    {
        Assert.Equal(expected, status.ToDisplayName());
    }

    [Theory]
    [InlineData(StockOperationType.Receipt, "Приём")]
    [InlineData(StockOperationType.WriteOff, "Списание")]
    [InlineData(StockOperationType.Issue, "Выдача")]
    public void StockOperationType_ToDisplayName_ReturnsRussianName(StockOperationType operationType, string expected)
    {
        Assert.Equal(expected, operationType.ToDisplayName());
    }
}
