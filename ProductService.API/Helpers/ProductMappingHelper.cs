using ProductService.API.DTOs;
using ProductService.API.Models;

namespace ProductService.API.Helpers
{
    public static class ProductMappingHelper
    {
        public static ProductResponse ToProductResponse(Product product, string? languageCode)
        {
            var language = LanguageCodes.Normalize(languageCode);

            var productTranslation = product.Translations
                .FirstOrDefault(t => t.LanguageCode == language)
                ?? product.Translations.FirstOrDefault(t => t.LanguageCode == LanguageCodes.English)
                ?? product.Translations.FirstOrDefault();

            var categoryTranslation = product.Category.Translations
                .FirstOrDefault(t => t.LanguageCode == language)
                ?? product.Category.Translations.FirstOrDefault(t => t.LanguageCode == LanguageCodes.English)
                ?? product.Category.Translations.FirstOrDefault();

            return new ProductResponse
            {
                Id = product.Id,
                SellerId = product.SellerId,
                CategoryId = product.CategoryId,
                CategoryName = categoryTranslation?.Name ?? product.Category.Slug,
                Sku = product.Sku,
                Name = productTranslation?.Name ?? product.Sku,
                ShortDescription = productTranslation?.ShortDescription,
                Description = productTranslation?.Description,
                Price = product.Price,
                CompareAtPrice = product.CompareAtPrice,
                Currency = product.Currency,
                IsActive = product.IsActive,
                IsDeleted = product.IsDeleted,
                Quantity = product.Inventory?.Quantity ?? 0,
                ReservedQuantity = product.Inventory?.ReservedQuantity ?? 0,
                AvailableQuantity = product.Inventory?.AvailableQuantity ?? 0,
                Images = product.Images
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageResponse
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        AltText = i.AltText,
                        IsMain = i.IsMain,
                        DisplayOrder = i.DisplayOrder
                    })
                    .ToList(),
                Variants = product.Variants
                    .OrderBy(v => v.CreatedAt)
                    .Select(v => new ProductVariantResponse
                    {
                        Id = v.Id,
                        Sku = v.Sku,
                        VariantName = v.VariantName,
                        Price = v.Price,
                        CompareAtPrice = v.CompareAtPrice,
                        IsActive = v.IsActive,
                        Quantity = v.Inventory?.Quantity ?? 0,
                        ReservedQuantity = v.Inventory?.ReservedQuantity ?? 0,
                        AvailableQuantity = v.Inventory?.AvailableQuantity ?? 0
                    })
                    .ToList(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}