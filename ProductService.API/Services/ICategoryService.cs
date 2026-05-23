using ProductService.API.DTOs;

namespace ProductService.API.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetCategoriesAsync(string? languageCode);
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid id, string? languageCode);
        Task<CategoryResponse?> GetCategoryBySlugAsync(string slug, string? languageCode);

        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, string? languageCode);
        Task<CategoryResponse?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, string? languageCode);
        Task<bool> ActivateCategoryAsync(Guid id);
        Task<bool> DeactivateCategoryAsync(Guid id);
    }
}