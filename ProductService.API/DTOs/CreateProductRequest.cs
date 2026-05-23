using System.ComponentModel.DataAnnotations;

namespace ProductService.API.DTOs
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Category ID is required.")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "SKU is required.")]
        [MaxLength(100, ErrorMessage = "SKU cannot be longer than 100 characters.")]
        public string Sku { get; set; } = string.Empty;

        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Compare at price cannot be negative.")]
        public decimal? CompareAtPrice { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [MaxLength(3, ErrorMessage = "Currency cannot be longer than 3 characters.")]
        public string Currency { get; set; } = "EUR";

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Translations are required.")]
        [MinLength(1, ErrorMessage = "At least one translation is required.")]
        public List<ProductTranslationRequest> Translations { get; set; } = new();

        public List<ProductImageRequest> Images { get; set; } = new();

        public List<ProductVariantRequest> Variants { get; set; } = new();
    }
}