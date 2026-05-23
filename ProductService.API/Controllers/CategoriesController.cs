using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.API.Helpers;
using ProductService.API.Services;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryResponse>>>> GetCategories(
            [FromQuery] string? language = "en")
        {
            var categories = await _categoryService.GetCategoriesAsync(language);

            return Ok(ApiResponse<List<CategoryResponse>>.Ok(
                categories,
                "Categories loaded successfully."
            ));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetCategoryById(
            Guid id,
            [FromQuery] string? language = "en")
        {
            var category = await _categoryService.GetCategoryByIdAsync(id, language);

            if (category == null)
            {
                return NotFound(ApiResponse<CategoryResponse>.Fail("Category not found."));
            }

            return Ok(ApiResponse<CategoryResponse>.Ok(
                category,
                "Category loaded successfully."
            ));
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetCategoryBySlug(
            string slug,
            [FromQuery] string? language = "en")
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug, language);

            if (category == null)
            {
                return NotFound(ApiResponse<CategoryResponse>.Fail("Category not found."));
            }

            return Ok(ApiResponse<CategoryResponse>.Ok(
                category,
                "Category loaded successfully."
            ));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> CreateCategory(
    CreateCategoryRequest request,
    [FromQuery] string? language = "en")
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(request, language);

                return Ok(ApiResponse<CategoryResponse>.Ok(
                    category,
                    "Category created successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CategoryResponse>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryResponse>>> UpdateCategory(
            Guid id,
            UpdateCategoryRequest request,
            [FromQuery] string? language = "en")
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, request, language);

                if (category == null)
                {
                    return NotFound(ApiResponse<CategoryResponse>.Fail("Category not found."));
                }

                return Ok(ApiResponse<CategoryResponse>.Ok(
                    category,
                    "Category updated successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CategoryResponse>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut("{id:guid}/activate")]
        public async Task<ActionResult<ApiResponse<object>>> ActivateCategory(Guid id)
        {
            var result = await _categoryService.ActivateCategoryAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Category not found."));
            }

            return Ok(ApiResponse<object>.Ok(
                null,
                "Category activated successfully."
            ));
        }

        [Authorize(Roles = RoleNames.Admin)]
        [HttpPut("{id:guid}/deactivate")]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateCategory(Guid id)
        {
            var result = await _categoryService.DeactivateCategoryAsync(id);

            if (!result)
            {
                return NotFound(ApiResponse<object>.Fail("Category not found."));
            }

            return Ok(ApiResponse<object>.Ok(
                null,
                "Category deactivated successfully."
            ));
        }
    }
}