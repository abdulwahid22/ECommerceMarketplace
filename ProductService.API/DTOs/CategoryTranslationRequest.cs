using System.ComponentModel.DataAnnotations;

namespace ProductService.API.DTOs
{
    public class CategoryTranslationRequest
    {
        [Required(ErrorMessage = "Language code is required.")]
        [MaxLength(10, ErrorMessage = "Language code cannot be longer than 10 characters.")]
        public string LanguageCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(200, ErrorMessage = "Category name cannot be longer than 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string? Description { get; set; }
    }
}