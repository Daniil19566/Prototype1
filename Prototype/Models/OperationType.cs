namespace Prototype.Models
{
    public enum StockOperationType
    {
        Receipt,
        WriteOff,
        Issue
    }

    public static class StockOperationTypeExtensions
    {
        public static string ToDisplayName(this StockOperationType operationType) => operationType switch
        {
            StockOperationType.Receipt => "Приём",
            StockOperationType.WriteOff => "Списание",
            StockOperationType.Issue => "Выдача",
            _ => operationType.ToString()
        };

        public static string ToDisplayName(this StockOperation operation) => operation.OperationType.ToDisplayName();
    }
}
