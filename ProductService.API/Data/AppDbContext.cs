using Microsoft.EntityFrameworkCore;
using ProductService.API.Models;

namespace ProductService.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Inventory> Inventories => Set<Inventory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Slug)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(c => c.Slug)
                    .IsUnique();

                entity.Property(c => c.IsActive)
                    .HasDefaultValue(true);

                entity.Property(c => c.DisplayOrder)
                    .HasDefaultValue(0);

                entity.Property(c => c.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.SubCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CategoryTranslation>(entity =>
            {
                entity.HasKey(ct => ct.Id);

                entity.Property(ct => ct.LanguageCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(ct => ct.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(ct => ct.Description)
                    .HasMaxLength(1000);

                entity.HasIndex(ct => new { ct.CategoryId, ct.LanguageCode })
                    .IsUnique();

                entity.HasOne(ct => ct.Category)
                    .WithMany(c => c.Translations)
                    .HasForeignKey(ct => ct.CategoryId);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Sku)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(p => p.Sku)
                    .IsUnique();

                entity.Property(p => p.Price)
                    .HasColumnType("numeric(18,2)");

                entity.Property(p => p.CompareAtPrice)
                    .HasColumnType("numeric(18,2)");

                entity.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("EUR");

                entity.Property(p => p.IsActive)
                    .HasDefaultValue(true);

                entity.Property(p => p.IsDeleted)
                    .HasDefaultValue(false);

                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId);
            });

            modelBuilder.Entity<ProductTranslation>(entity =>
            {
                entity.HasKey(pt => pt.Id);

                entity.Property(pt => pt.LanguageCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(pt => pt.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(pt => pt.ShortDescription)
                    .HasMaxLength(500);

                entity.Property(pt => pt.Description)
                    .HasMaxLength(5000);

                entity.HasIndex(pt => new { pt.ProductId, pt.LanguageCode })
                    .IsUnique();

                entity.HasOne(pt => pt.Product)
                    .WithMany(p => p.Translations)
                    .HasForeignKey(pt => pt.ProductId);
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(pi => pi.Id);

                entity.Property(pi => pi.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(pi => pi.AltText)
                    .HasMaxLength(250);

                entity.Property(pi => pi.IsMain)
                    .HasDefaultValue(false);

                entity.Property(pi => pi.DisplayOrder)
                    .HasDefaultValue(0);

                entity.Property(pi => pi.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.ProductId);
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(pv => pv.Id);

                entity.Property(pv => pv.Sku)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(pv => pv.Sku)
                    .IsUnique();

                entity.Property(pv => pv.VariantName)
                    .HasMaxLength(250);

                entity.Property(pv => pv.Price)
                    .HasColumnType("numeric(18,2)");

                entity.Property(pv => pv.CompareAtPrice)
                    .HasColumnType("numeric(18,2)");

                entity.Property(pv => pv.IsActive)
                    .HasDefaultValue(true);

                entity.Property(pv => pv.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(pv => pv.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(pv => pv.ProductId);
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Quantity)
                    .IsRequired();

                entity.Property(i => i.ReservedQuantity)
                    .HasDefaultValue(0);

                entity.Property(i => i.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasOne(i => i.Product)
                    .WithOne(p => p.Inventory)
                    .HasForeignKey<Inventory>(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.ProductVariant)
                    .WithOne(pv => pv.Inventory)
                    .HasForeignKey<Inventory>(i => i.ProductVariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}