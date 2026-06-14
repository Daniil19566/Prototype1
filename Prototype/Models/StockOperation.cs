namespace Prototype.Models
{
    public class StockOperation
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public StockOperationType OperationType { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Comment { get; set; } = string.Empty;
    }
}
