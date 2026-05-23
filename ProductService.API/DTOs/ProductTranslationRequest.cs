using System.ComponentModel.DataAnnotations;

namespace ProductService.API.DTOs
{
    public class ProductTranslationRequest
    {
        [Required(ErrorMessage = "Language code is required.")]
        [MaxLength(10, ErrorMessage = "Language code cannot be longer than 10 characters.")]
        public string LanguageCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(250, ErrorMessage = "Product name cannot be longer than 250 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Short description cannot be longer than 500 characters.")]
        public string? ShortDescription { get; set; }

        [MaxLength(5000, ErrorMessage = "Description cannot be longer than 5000 characters.")]
        public string? Description { get; set; }
    }
}