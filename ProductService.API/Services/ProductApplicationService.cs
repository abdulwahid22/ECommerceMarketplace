using Microsoft.EntityFrameworkCore;
using ProductService.API.Data;
using ProductService.API.DTOs;
using ProductService.API.Helpers;
using ProductService.API.Models;

namespace ProductService.API.Services
{
    public class ProductApplicationService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductApplicationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductResponse> CreateProductAsync(
            Guid sellerId,
            CreateProductRequest request,
            string? languageCode)
        {
            ValidateTranslations(request.Translations);

            var sku = NormalizeSku(request.Sku);
            var currency = request.Currency.Trim().ToUpper();

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId && c.IsActive);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Category not found.");
            }

            var skuExists = await _context.Products
                .AnyAsync(p => p.Sku.ToLower() == sku);

            if (skuExists)
            {
                throw new InvalidOperationException("Product SKU already exists.");
            }

            var duplicateVariantSku = request.Variants
                .GroupBy(v => NormalizeSku(v.Sku))
                .Any(g => g.Count() > 1);

            if (duplicateVariantSku)
            {
                throw new InvalidOperationException("Duplicate variant SKU is not allowed.");
            }

            foreach (var variant in request.Variants)
            {
                var variantSku = NormalizeSku(variant.Sku);

                var variantSkuExists = await _context.ProductVariants
                    .AnyAsync(v => v.Sku.ToLower() == variantSku);

                if (variantSkuExists)
                {
                    throw new InvalidOperationException($"Variant SKU '{variant.Sku}' already exists.");
                }
            }

            var productId = Guid.NewGuid();

            var product = new Product
            {
                Id = productId,
                SellerId = sellerId,
                CategoryId = request.CategoryId,
                Sku = sku,
                Price = request.Price,
                CompareAtPrice = request.CompareAtPrice,
                Currency = currency,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Translations = request.Translations.Select(t => new ProductTranslation
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    LanguageCode = t.LanguageCode.Trim().ToLower(),
                    Name = t.Name.Trim(),
                    ShortDescription = string.IsNullOrWhiteSpace(t.ShortDescription) ? null : t.ShortDescription.Trim(),
                    Description = string.IsNullOrWhiteSpace(t.Description) ? null : t.Description.Trim()
                }).ToList(),
                Images = request.Images.Select((i, index) => new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ImageUrl = i.ImageUrl.Trim(),
                    AltText = string.IsNullOrWhiteSpace(i.AltText) ? null : i.AltText.Trim(),
                    IsMain = i.IsMain,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
            };

            if (product.Images.Any() && !product.Images.Any(i => i.IsMain))
            {
                product.Images.First().IsMain = true;
            }

            if (request.Variants.Any())
            {
                foreach (var variantRequest in request.Variants)
                {
                    var variantId = Guid.NewGuid();

                    var variant = new ProductVariant
                    {
                        Id = variantId,
                        ProductId = productId,
                        Sku = NormalizeSku(variantRequest.Sku),
                        VariantName = string.IsNullOrWhiteSpace(variantRequest.VariantName)
                            ? null
                            : variantRequest.VariantName.Trim(),
                        Price = variantRequest.Price,
                        CompareAtPrice = variantRequest.CompareAtPrice,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Inventory = new Inventory
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = variantId,
                            Quantity = variantRequest.Quantity,
                            ReservedQuantity = 0,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    product.Variants.Add(variant);
                }
            }
            else
            {
                product.Inventory = new Inventory
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = request.Quantity,
                    ReservedQuantity = 0,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            var createdProduct = await GetProductEntityQuery()
                .FirstAsync(p => p.Id == product.Id);

            return ProductMappingHelper.ToProductResponse(createdProduct, languageCode);
        }

        public async Task<PagedResponse<ProductResponse>> GetProductsAsync(ProductQueryParameters query)
        {
            if (query.PageNumber < 1)
            {
                query.PageNumber = 1;
            }

            if (query.PageSize < 1)
            {
                query.PageSize = 10;
            }

            var language = LanguageCodes.Normalize(query.Language);

            var productsQuery = GetProductEntityQuery()
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();

                productsQuery = productsQuery.Where(p =>
                    p.Sku.ToLower().Contains(search) ||
                    p.Translations.Any(t =>
                        t.Name.ToLower().Contains(search) ||
                        (t.Description != null && t.Description.ToLower().Contains(search)) ||
                        (t.ShortDescription != null && t.ShortDescription.ToLower().Contains(search))
                    )
                );
            }

            if (query.CategoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            if (query.SellerId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.SellerId == query.SellerId.Value);
            }

            if (query.MinPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
            }

            if (query.IsActive.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.IsActive == query.IsActive.Value);
            }

            var totalCount = await productsQuery.CountAsync();

            var products = await productsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = products
                .Select(p => ProductMappingHelper.ToProductResponse(p, language))
                .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<ProductResponse>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<ProductResponse?> GetProductByIdAsync(Guid id, string? languageCode)
        {
            var product = await GetProductEntityQuery()
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null)
            {
                return null;
            }

            return ProductMappingHelper.ToProductResponse(product, languageCode);
        }

        public async Task<ProductResponse?> GetProductBySkuAsync(string sku, string? languageCode)
        {
            var normalizedSku = NormalizeSku(sku);

            var product = await GetProductEntityQuery()
                .FirstOrDefaultAsync(p => p.Sku.ToLower() == normalizedSku && !p.IsDeleted);

            if (product == null)
            {
                return null;
            }

            return ProductMappingHelper.ToProductResponse(product, languageCode);
        }

        public async Task<ProductResponse?> UpdateProductAsync(
      Guid productId,
      Guid currentUserId,
      bool isAdmin,
      UpdateProductRequest request,
      string? languageCode)
        {
            ValidateTranslations(request.Translations);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

            if (product == null)
            {
                return null;
            }

            if (!isAdmin && product.SellerId != currentUserId)
            {
                throw new InvalidOperationException("You are not allowed to update this product.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.CategoryId && c.IsActive);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Category not found.");
            }

            var sku = NormalizeSku(request.Sku);
            var currency = request.Currency.Trim().ToUpper();

            var skuExists = await _context.Products
                .AnyAsync(p => p.Id != productId && p.Sku.ToLower() == sku);

            if (skuExists)
            {
                throw new InvalidOperationException("Product SKU already exists.");
            }

            var duplicateVariantSku = request.Variants
                .GroupBy(v => NormalizeSku(v.Sku))
                .Any(g => g.Count() > 1);

            if (duplicateVariantSku)
            {
                throw new InvalidOperationException("Duplicate variant SKU is not allowed.");
            }

            foreach (var variantRequest in request.Variants)
            {
                var variantSku = NormalizeSku(variantRequest.Sku);

                var variantSkuExists = await _context.ProductVariants
                    .AnyAsync(v =>
                        v.ProductId != product.Id &&
                        v.Sku.ToLower() == variantSku);

                if (variantSkuExists)
                {
                    throw new InvalidOperationException($"Variant SKU '{variantRequest.Sku}' already exists.");
                }
            }

            product.CategoryId = request.CategoryId;
            product.Sku = sku;
            product.Price = request.Price;
            product.CompareAtPrice = request.CompareAtPrice;
            product.Currency = currency;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.ProductTranslations
                .Where(t => t.ProductId == product.Id)
                .ExecuteDeleteAsync();

            await _context.ProductImages
                .Where(i => i.ProductId == product.Id)
                .ExecuteDeleteAsync();

            var translations = request.Translations.Select(t => new ProductTranslation
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                LanguageCode = t.LanguageCode.Trim().ToLower(),
                Name = t.Name.Trim(),
                ShortDescription = string.IsNullOrWhiteSpace(t.ShortDescription)
                    ? null
                    : t.ShortDescription.Trim(),
                Description = string.IsNullOrWhiteSpace(t.Description)
                    ? null
                    : t.Description.Trim()
            }).ToList();

            await _context.ProductTranslations.AddRangeAsync(translations);

            var images = request.Images.Select(i => new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = i.ImageUrl.Trim(),
                AltText = string.IsNullOrWhiteSpace(i.AltText)
                    ? null
                    : i.AltText.Trim(),
                IsMain = i.IsMain,
                DisplayOrder = i.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            if (images.Any() && !images.Any(i => i.IsMain))
            {
                images.First().IsMain = true;
            }

            await _context.ProductImages.AddRangeAsync(images);

            var oldVariantIds = await _context.ProductVariants
                .Where(v => v.ProductId == product.Id)
                .Select(v => v.Id)
                .ToListAsync();

            if (oldVariantIds.Any())
            {
                await _context.Inventories
                    .Where(i => i.ProductVariantId.HasValue &&
                                oldVariantIds.Contains(i.ProductVariantId.Value))
                    .ExecuteDeleteAsync();

                await _context.ProductVariants
                    .Where(v => v.ProductId == product.Id)
                    .ExecuteDeleteAsync();
            }

            if (request.Variants.Any())
            {
                await _context.Inventories
                    .Where(i => i.ProductId == product.Id)
                    .ExecuteDeleteAsync();

                var variants = new List<ProductVariant>();

                foreach (var variantRequest in request.Variants)
                {
                    var variantId = Guid.NewGuid();

                    var variant = new ProductVariant
                    {
                        Id = variantId,
                        ProductId = product.Id,
                        Sku = NormalizeSku(variantRequest.Sku),
                        VariantName = string.IsNullOrWhiteSpace(variantRequest.VariantName)
                            ? null
                            : variantRequest.VariantName.Trim(),
                        Price = variantRequest.Price,
                        CompareAtPrice = variantRequest.CompareAtPrice,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Inventory = new Inventory
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = variantId,
                            Quantity = variantRequest.Quantity,
                            ReservedQuantity = 0,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    variants.Add(variant);
                }

                await _context.ProductVariants.AddRangeAsync(variants);
            }
            else
            {
                var productInventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == product.Id);

                if (productInventory == null)
                {
                    var inventory = new Inventory
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Quantity = request.Quantity,
                        ReservedQuantity = 0,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.Inventories.AddAsync(inventory);
                }
                else
                {
                    productInventory.Quantity = request.Quantity;
                    productInventory.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _context.ChangeTracker.Clear();

            var updatedProduct = await GetProductEntityQuery()
                .FirstAsync(p => p.Id == product.Id);

            return ProductMappingHelper.ToProductResponse(updatedProduct, languageCode);
        }

        private IQueryable<Product> GetProductEntityQuery()
        {
            return _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Translations)
                .Include(p => p.Translations)
                .Include(p => p.Images)
                .Include(p => p.Inventory)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Inventory);
        }

        private void ValidateTranslations(List<ProductTranslationRequest> translations)
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

            if (!languageCodes.Contains(LanguageCodes.English))
            {
                throw new InvalidOperationException("English translation is required.");
            }
        }

        private string NormalizeSku(string sku)
        {
            return sku.Trim().ToLower();
        }
    }
}