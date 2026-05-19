namespace ProductService.API.Models
{
    public class Inventory
    {
        public Guid Id { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? ProductVariantId { get; set; }

        public int Quantity { get; set; }

        public int ReservedQuantity { get; set; } = 0;

        public int AvailableQuantity => Quantity - ReservedQuantity;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }

        public ProductVariant? ProductVariant { get; set; }
    }
}