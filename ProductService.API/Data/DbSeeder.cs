using Microsoft.EntityFrameworkCore;
using ProductService.API.Helpers;
using ProductService.API.Models;

namespace ProductService.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            await SeedCategoriesAsync(context);
        }

        private static async Task SeedCategoriesAsync(AppDbContext context)
        {
            if (await context.Categories.AnyAsync())
            {
                return;
            }

            var categories = new List<Category>
            {
                CreateCategory(
                    slug: "electronics",
                    displayOrder: 1,
                    englishName: "Electronics",
                    englishDescription: "Electronic devices, gadgets, computers, and accessories.",
                    germanName: "Elektronik",
                    germanDescription: "Elektronische Geräte, Gadgets, Computer und Zubehör."
                ),

                CreateCategory(
                    slug: "clothing",
                    displayOrder: 2,
                    englishName: "Clothing",
                    englishDescription: "Men's, women's, and children's clothing.",
                    germanName: "Kleidung",
                    germanDescription: "Kleidung für Männer, Frauen und Kinder."
                ),

                CreateCategory(
                    slug: "home-kitchen",
                    displayOrder: 3,
                    englishName: "Home & Kitchen",
                    englishDescription: "Home goods, kitchen tools, furniture, and household items.",
                    germanName: "Haushalt & Küche",
                    germanDescription: "Haushaltswaren, Küchenutensilien, Möbel und Wohnartikel."
                ),

                CreateCategory(
                    slug: "books",
                    displayOrder: 4,
                    englishName: "Books",
                    englishDescription: "Books, educational materials, and reading products.",
                    germanName: "Bücher",
                    germanDescription: "Bücher, Lernmaterialien und Leseprodukte."
                ),

                CreateCategory(
                    slug: "sports",
                    displayOrder: 5,
                    englishName: "Sports",
                    englishDescription: "Sports equipment, fitness products, and outdoor gear.",
                    germanName: "Sport",
                    germanDescription: "Sportausrüstung, Fitnessprodukte und Outdoor-Ausrüstung."
                ),

                CreateCategory(
                    slug: "beauty",
                    displayOrder: 6,
                    englishName: "Beauty",
                    englishDescription: "Beauty, skincare, and personal care products.",
                    germanName: "Beauty",
                    germanDescription: "Beauty-, Hautpflege- und Körperpflegeprodukte."
                )
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        private static Category CreateCategory(
            string slug,
            int displayOrder,
            string englishName,
            string englishDescription,
            string germanName,
            string germanDescription)
        {
            var categoryId = Guid.NewGuid();

            return new Category
            {
                Id = categoryId,
                Slug = slug,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Translations = new List<CategoryTranslation>
                {
                    new CategoryTranslation
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = categoryId,
                        LanguageCode = LanguageCodes.English,
                        Name = englishName,
                        Description = englishDescription
                    },
                    new CategoryTranslation
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = categoryId,
                        LanguageCode = LanguageCodes.German,
                        Name = germanName,
                        Description = germanDescription
                    }
                }
            };
        }
    }
}