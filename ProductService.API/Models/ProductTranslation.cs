namespace ProductService.API.Models
{
    public class ProductTranslation
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        public Product Product { get; set; } = null!;
    }
}