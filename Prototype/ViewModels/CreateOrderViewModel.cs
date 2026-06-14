using System.ComponentModel.DataAnnotations;

namespace Prototype.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
