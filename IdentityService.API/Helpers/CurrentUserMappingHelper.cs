using IdentityService.API.DTOs;
using IdentityService.API.Models;

namespace IdentityService.API.Helpers
{
    public static class CurrentUserMappingHelper
    {
        public static CurrentUserResponse ToCurrentUserResponse(User user)
        {
            return new CurrentUserResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = user.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            };
        }
    }
}