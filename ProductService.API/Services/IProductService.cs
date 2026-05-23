using ProductService.API.DTOs;

namespace ProductService.API.Services
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(Guid sellerId, CreateProductRequest request, string? languageCode);
        Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductQueryParameters query);
        Task<ProductResponse?> GetProductByIdAsync(Guid id, string? languageCode);
        Task<ProductResponse?> GetProductBySkuAsync(string sku, string? languageCode);
        Task<ProductResponse?> UpdateProductAsync(
    Guid productId,
    Guid currentUserId,
    bool isAdmin,
    UpdateProductRequest request,
    string? languageCode);
    }
}