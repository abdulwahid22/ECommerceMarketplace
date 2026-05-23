using Microsoft.EntityFrameworkCore;
using ProductService.API.Data;
using ProductService.API.DTOs;
using ProductService.API.Helpers;
using ProductService.API.Models;
namespace ProductService.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryResponse>> GetCategoriesAsync(string? languageCode)
        {
            var language = LanguageCodes.Normalize(languageCode);

            var categories = await _context.Categories
                .Include(c => c.Translations)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Translations)
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories
                .Select(c => CategoryMappingHelper.ToCategoryResponse(c, language))
                .ToList();
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id, string? languageCode)
        {
            var language = LanguageCodes.Normalize(languageCode);

            var category = await _context.Categories
                .Include(c => c.Translations)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Translations)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
            {
                return null;
            }

            return CategoryMappingHelper.ToCategoryResponse(category, language);
        }

        public async Task<CategoryResponse?> GetCategoryBySlugAsync(string slug, string? languageCode)
        {
            var language = LanguageCodes.Normalize(languageCode);

            var normalizedSlug = slug.Trim().ToLower();

            var category = await _context.Categories
                .Include(c => c.Translations)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Translations)
                .FirstOrDefaultAsync(c => c.Slug.ToLower() == normalizedSlug && c.IsActive);

            if (category == null)
            {
                return null;
            }

            return CategoryMappingHelper.ToCategoryResponse(category, language);
        }
        public async Task<CategoryResponse> CreateCategoryAsync(
    CreateCategoryRequest request,
    string? languageCode)
        {
            ValidateTranslations(request.Translations);

            var slug = NormalizeSlug(request.Slug);

            var slugExists = await _context.Categories
                .AnyAsync(c => c.Slug.ToLower() == slug);

            if (slugExists)
            {
                throw new InvalidOperationException("Category slug already exists.");
            }

            if (request.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories
                  .AnyAsync(c => c.Id == request.ParentCategoryId.Value && c.IsActive && c.ParentCategoryId == null);

                if (!parentExists)
                {
                    throw new InvalidOperationException("Parent category not found.");
                }
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                ParentCategoryId = request.ParentCategoryId,
                Slug = slug,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Translations = request.Translations.Select(t => new CategoryTranslation
                {
                    Id = Guid.NewGuid(),
                    LanguageCode = t.LanguageCode.Trim().ToLower(),
                    Name = t.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(t.Description) ? null : t.Description.Trim()
                }).ToList()
            };

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            var createdCategory = await _context.Categories
                .Include(c => c.Translations)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Translations)
                .FirstAsync(c => c.Id == category.Id);

            var language = LanguageCodes.Normalize(languageCode);

            return CategoryMappingHelper.ToCategoryResponse(createdCategory, language);
        }
        public async Task<CategoryResponse?> UpdateCategoryAsync(
    Guid id,
    UpdateCategoryRequest request,
    string? languageCode)
        {
            ValidateTranslations(request.Translations);

            var category = await _context.Categories
                .Include(c => c.Translations)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.Translations)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return null;
            }

            var slug = NormalizeSlug(request.Slug);

            var slugExists = await _context.Categories
                .AnyAsync(c => c.Id != id && c.Slug.ToLower() == slug);

            if (slugExists)
            {
                throw new InvalidOperationException("Category slug already exists.");
            }

            if (request.ParentCategoryId == id)
            {
                throw new InvalidOperationException("A category cannot be its own parent.");
            }

            if (request.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories
                    .AnyAsync(c =>
                        c.Id == request.ParentCategoryId.Value &&
                        c.IsActive &&
                        c.ParentCategoryId == null);

                if (!parentExists)
                {
                    throw new InvalidOperationException("Parent category not found.");
                }
            }

            category.ParentCategoryId = request.ParentCategoryId;
            category.Slug = slug;
            category.DisplayOrder = request.DisplayOrder;
            category.UpdatedAt = DateTime.UtcNow;

            _context.CategoryTranslations.RemoveRange(category.Translations);

            category.Translations = request.Translations.Select(t => new CategoryTranslation
            {
                Id = Guid.NewGuid(),
                CategoryId = category.Id,
                LanguageCode = t.LanguageCode.Trim().ToLower(),
                Name = t.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(t.Description) ? null : t.Description.Trim()
            }).ToList();

            await _context.SaveChangesAsync();

            var language = LanguageCodes.Normalize(languageCode);

            return CategoryMappingHelper.ToCategoryResponse(category, language);
        }

        public async Task<bool> ActivateCategoryAsync(Guid id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false;
            }

            category.IsActive = true;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateCategoryAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false;
            }

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;

            foreach (var subCategory in category.SubCategories)
            {
                subCategory.IsActive = false;
                subCategory.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return true;
        }
        private void ValidateTranslations(List<CategoryTranslationRequest> translations)
        {
            if (translations == null || !translations.Any())
            {
                throw new InvalidOperationException("At least one translation is required.");
            }

            var languageCodes = translations
                .Select(t => t.LanguageCode.Trim().ToLower())
                .ToList();

            if (languageCodes.Count != languageCodes.Distinct().Count())
            {
                throw new InvalidOperationException("Duplicate language translations are not allowed.");
            }

            foreach (var translation in translations)
            {
                var languageCode = translation.LanguageCode.Trim().ToLower();

                if (!LanguageCodes.IsSupported(languageCode))
                {
                    throw new InvalidOperationException($"Language '{translation.LanguageCode}' is not supported.");
                }
            }

            var hasEnglish = languageCodes.Contains(LanguageCodes.English);

            if (!hasEnglish)
            {
                throw new InvalidOperationException("English translation is required.");
            }
        }

        private string NormalizeSlug(string slug)
        {
            return slug.Trim().ToLower();
        }
    }
}