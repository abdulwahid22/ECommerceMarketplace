namespace ProductService.API.DTOs
{
    public class ProductVariantResponse
    {
        public Guid Id { get; set; }

        public string Sku { get; set; } = string.Empty;

        public string? VariantName { get; set; }

        public decimal Price { get; set; }

        public decimal? CompareAtPrice { get; set; }

        public bool IsActive { get; set; }

        public int Quantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int AvailableQuantity { get; set; }
    }
}