using IdentityService.API.Helpers;
using IdentityService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            await SeedRolesAsync(context);
            await SeedDevelopmentAdminAsync(context);
        }

        private static async Task SeedRolesAsync(AppDbContext context)
        {
            var roleNames = new List<string>
            {
                RoleNames.Admin,
                RoleNames.Seller,
                RoleNames.Customer
            };

            foreach (var roleName in roleNames)
            {
                var roleExists = await context.Roles
                    .AnyAsync(r => r.Name == roleName);

                if (!roleExists)
                {
                    context.Roles.Add(new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedDevelopmentAdminAsync(AppDbContext context)
        {
            var adminEmail = "a.shafayee@gmail.com";
            var adminPassword = "Admin@123786";

            var adminExists = await context.Users
                .AnyAsync(u => u.Email == adminEmail);

            if (adminExists)
            {
                return;
            }

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == RoleNames.Admin);

            if (adminRole == null)
            {
                throw new InvalidOperationException("Admin role does not exist.");
            }

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "System",
                LastName = "Admin",
                Email = adminEmail,
                EmailConfirmed = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var adminUserRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            };

            context.Users.Add(adminUser);
            context.UserRoles.Add(adminUserRole);

            await context.SaveChangesAsync();
        }
    }
}