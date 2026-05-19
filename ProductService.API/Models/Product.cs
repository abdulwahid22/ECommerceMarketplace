namespace ProductService.API.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public Guid SellerId { get; set; }

        public Guid CategoryId { get; set; }

        public string Sku { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? CompareAtPrice { get; set; }

        public string Currency { get; set; } = "EUR";

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Category Category { get; set; } = null!;

        public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        public Inventory? Inventory { get; set; }
    }
}