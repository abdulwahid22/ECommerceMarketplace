namespace ProductService.API.DTOs
{
    public class ProductImageResponse
    {
        public Guid Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? AltText { get; set; }

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; }
    }
}