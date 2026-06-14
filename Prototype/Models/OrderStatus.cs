namespace Prototype.Models
{
    public enum OrderStatus
    {
        Created,
        InAssembly,
        ReadyForPickup,
        Issued
    }

    public static class OrderStatusExtensions
    {
        public static string ToDisplayName(this OrderStatus status) => status switch
        {
            OrderStatus.Created => "Создан",
            OrderStatus.InAssembly => "В сборке",
            OrderStatus.ReadyForPickup => "Готов к выдаче",
            OrderStatus.Issued => "Выдан",
            _ => status.ToString()
        };

        public static string ToDisplayName(this Order order) => order.Status.ToDisplayName();

        public static bool CanMoveToAssembly(this Order order) => order.Status == OrderStatus.Created;

        public static bool CanConfirmAssembly(this Order order) => order.Status == OrderStatus.InAssembly;

        public static bool CanIssue(this Order order) => order.Status == OrderStatus.ReadyForPickup;
    }
}
