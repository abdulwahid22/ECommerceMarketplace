namespace ProductService.API.Models
{
    public class CategoryTranslation
    {
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Category Category { get; set; } = null!;
    }
}