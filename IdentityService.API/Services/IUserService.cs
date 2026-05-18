using IdentityService.API.DTOs;

namespace IdentityService.API.Services
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetUsersAsync(UserQueryParameters query);
        Task<UserResponse?> GetUserByIdAsync(Guid id);
        Task<bool> ActivateUserAsync(Guid id, Guid performedByUserId);
        Task<bool> DeactivateUserAsync(Guid id, Guid performedByUserId);
        Task<bool> SoftDeleteUserAsync(Guid id, Guid performedByUserId);
        Task<bool> UpdateUserRoleAsync(Guid id, UpdateUserRoleRequest request, Guid performedByUserId);
    }
}