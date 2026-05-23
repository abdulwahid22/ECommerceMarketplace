using System.ComponentModel.DataAnnotations;

namespace ProductService.API.DTOs
{
    public class ProductVariantRequest
    {
        [Required(ErrorMessage = "Variant SKU is required.")]
        [MaxLength(100, ErrorMessage = "Variant SKU cannot be longer than 100 characters.")]
        public string Sku { get; set; } = string.Empty;

        [MaxLength(250, ErrorMessage = "Variant name cannot be longer than 250 characters.")]
        public string? VariantName { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Variant price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Compare at price cannot be negative.")]
        public decimal? CompareAtPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }
    }
}