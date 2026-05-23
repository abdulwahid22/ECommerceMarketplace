using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.API.Helpers;
using ProductService.API.Services;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProductResponse>>>> GetProducts(
            [FromQuery] ProductQueryParameters query)
        {
            var products = await _productService.GetProductsAsync(query);

            return Ok(ApiResponse<PagedResponse<ProductResponse>>.Ok(
                products,
                "Products loaded successfully."
            ));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProductById(
            Guid id,
            [FromQuery] string? language = "en")
        {
            var product = await _productService.GetProductByIdAsync(id, language);

            if (product == null)
            {
                return NotFound(ApiResponse<ProductResponse>.Fail("Product not found."));
            }

            return Ok(ApiResponse<ProductResponse>.Ok(
                product,
                "Product loaded successfully."
            ));
        }

        [HttpGet("sku/{sku}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProductBySku(
            string sku,
            [FromQuery] string? language = "en")
        {
            var product = await _productService.GetProductBySkuAsync(sku, language);

            if (product == null)
            {
                return NotFound(ApiResponse<ProductResponse>.Fail("Product not found."));
            }

            return Ok(ApiResponse<ProductResponse>.Ok(
                product,
                "Product loaded successfully."
            ));
        }

        [Authorize(Roles = $"{RoleNames.Seller},{RoleNames.Admin}")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> CreateProduct(
            CreateProductRequest request,
            [FromQuery] string? language = "en")
        {
            var sellerId = CurrentUserHelper.GetUserId(User);

            if (sellerId == null)
            {
                return Unauthorized(ApiResponse<ProductResponse>.Fail("Invalid token."));
            }

            try
            {
                var product = await _productService.CreateProductAsync(
                    sellerId.Value,
                    request,
                    language
                );

                return Ok(ApiResponse<ProductResponse>.Ok(
                    product,
                    "Product created successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductResponse>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = $"{RoleNames.Seller},{RoleNames.Admin}")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductResponse>>> UpdateProduct(
    Guid id,
    UpdateProductRequest request,
    [FromQuery] string? language = "en")
        {
            var currentUserId = CurrentUserHelper.GetUserId(User);

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<ProductResponse>.Fail("Invalid token."));
            }

            var isAdmin = User.IsInRole(RoleNames.Admin);

            try
            {
                var product = await _productService.UpdateProductAsync(
                    id,
                    currentUserId.Value,
                    isAdmin,
                    request,
                    language
                );

                if (product == null)
                {
                    return NotFound(ApiResponse<ProductResponse>.Fail("Product not found."));
                }

                return Ok(ApiResponse<ProductResponse>.Ok(
                    product,
                    "Product updated successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductResponse>.Fail(ex.Message));
            }
        }
    }
}