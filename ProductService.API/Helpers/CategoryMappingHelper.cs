using ProductService.API.DTOs;
using ProductService.API.Models;

namespace ProductService.API.Helpers
{
    public static class CategoryMappingHelper
    {
        public static CategoryResponse ToCategoryResponse(Category category, string languageCode)
        {
            var normalizedLanguage = LanguageCodes.Normalize(languageCode);

            var translation = category.Translations
                .FirstOrDefault(t => t.LanguageCode == normalizedLanguage)
                ?? category.Translations.FirstOrDefault(t => t.LanguageCode == LanguageCodes.English)
                ?? category.Translations.FirstOrDefault();

            return new CategoryResponse
            {
                Id = category.Id,
                ParentCategoryId = category.ParentCategoryId,
                Slug = category.Slug,
                Name = translation?.Name ?? category.Slug,
                Description = translation?.Description,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder,
                SubCategories = category.SubCategories
                    .Where(sc => sc.IsActive)
                    .OrderBy(sc => sc.DisplayOrder)
                    .Select(sc => ToCategoryResponse(sc, normalizedLanguage))
                    .ToList()
            };
        }
    }
}