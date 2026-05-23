using System.ComponentModel.DataAnnotations;

namespace ProductService.API.DTOs
{
    public class ProductImageRequest
    {
        [Required(ErrorMessage = "Image URL is required.")]
        [MaxLength(1000, ErrorMessage = "Image URL cannot be longer than 1000 characters.")]
        public string ImageUrl { get; set; } = string.Empty;

        [MaxLength(250, ErrorMessage = "Alt text cannot be longer than 250 characters.")]
        public string? AltText { get; set; }

        public bool IsMain { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;
    }
}