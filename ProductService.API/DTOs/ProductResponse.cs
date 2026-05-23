namespace ProductService.API.DTOs
{
    public class ProductResponse
    {
        public Guid Id { get; set; }

        public Guid SellerId { get; set; }

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public decimal? CompareAtPrice { get; set; }

        public string Currency { get; set; } = "EUR";

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public int Quantity { get; set; }

        public int ReservedQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public List<ProductImageResponse> Images { get; set; } = new();

        public List<ProductVariantResponse> Variants { get; set; } = new();

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}