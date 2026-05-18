using IdentityService.API.DTOs;
using IdentityService.API.Models;

namespace IdentityService.API.Helpers
{
    public static class UserMappingHelper
    {
        public static UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                CreatedByUserId = user.CreatedByUserId,
                UpdatedAt = user.UpdatedAt,
                UpdatedByUserId = user.UpdatedByUserId,
                DeletedAt = user.DeletedAt,
                DeletedByUserId = user.DeletedByUserId,
                Roles = user.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            };
        }
    }
}