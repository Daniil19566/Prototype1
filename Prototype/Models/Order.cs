namespace Prototype.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public List<OrderItem> Items { get; set; } = [];
        public string? RecipientDocument { get; set; }
    }
}
