namespace ProductService.API.DTOs
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public string Slug { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public List<CategoryResponse> SubCategories { get; set; } = new();
    }
}