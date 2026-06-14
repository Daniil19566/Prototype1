using System.ComponentModel.DataAnnotations;

namespace Prototype.ViewModels
{
    public class ReceiptViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime Date { get; set; } = DateTime.Today;
    }
}
