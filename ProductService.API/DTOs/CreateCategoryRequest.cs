using System.ComponentModel.DataAnnotations;

namespace ProductService.API.DTOs
{
    public class CreateCategoryRequest
    {
        public Guid? ParentCategoryId { get; set; }

        [Required(ErrorMessage = "Slug is required.")]
        [MaxLength(200, ErrorMessage = "Slug cannot be longer than 200 characters.")]
        public string Slug { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        [Required(ErrorMessage = "Translations are required.")]
        [MinLength(1, ErrorMessage = "At least one translation is required.")]
        public List<CategoryTranslationRequest> Translations { get; set; } = new();
    }
}