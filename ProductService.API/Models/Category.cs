namespace ProductService.API.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public string Slug { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Category? ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<CategoryTranslation> Translations { get; set; } = new List<CategoryTranslation>();

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}