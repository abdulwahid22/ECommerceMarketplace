using IdentityService.API.Data;
using IdentityService.API.DTOs;
using Microsoft.EntityFrameworkCore;
using IdentityService.API.Helpers;
using IdentityService.API.Models;
namespace IdentityService.API.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public UserService(AppDbContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        public async Task<PagedResponse<UserResponse>> GetUsersAsync(UserQueryParameters query)
        {
            if (query.PageNumber < 1)
            {
                query.PageNumber = 1;
            }

            if (query.PageSize < 1)
            {
                query.PageSize = 10;
            }

            var usersQuery = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();

                usersQuery = usersQuery.Where(u =>
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                var role = query.Role.Trim().ToLower();

                usersQuery = usersQuery.Where(u =>
                    u.UserRoles.Any(ur => ur.Role.Name.ToLower() == role)
                );
            }

            if (query.IsActive.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsActive == query.IsActive.Value);
            }

            if (query.IsDeleted.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsDeleted == query.IsDeleted.Value);
            }

            var totalCount = await usersQuery.CountAsync();

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = users
     .Select(UserMappingHelper.ToUserResponse)
     .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResponse<UserResponse>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<UserResponse?> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return null;
            }

            return UserMappingHelper.ToUserResponse(user);
        }
        public async Task<bool> ActivateUserAsync(Guid id, Guid performedByUserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            if (user.IsDeleted)
            {
                throw new InvalidOperationException("Cannot activate a deleted user.");
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = performedByUserId;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                performedByUserId,
                AuditActions.UserActivated,
                EntityNames.User,
                user.Id,
                oldValues: "IsActive=false",
                newValues: "IsActive=true",
                description: $"User {user.Email} was activated."
            );

            return true;
        }

        public async Task<bool> DeactivateUserAsync(Guid id, Guid performedByUserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            if (await IsLastActiveAdminAsync(user.Id))
            {
                throw new InvalidOperationException("Cannot deactivate the last active admin account.");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = performedByUserId;

            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
    performedByUserId,
    AuditActions.UserDeactivated,
    EntityNames.User,
    user.Id,
    oldValues: "IsActive=true",
    newValues: "IsActive=false",
    description: $"User {user.Email} was deactivated."
);
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(Guid id, Guid performedByUserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            if (await IsLastActiveAdminAsync(user.Id))
            {
                throw new InvalidOperationException("Cannot delete the last active admin account.");
            }

            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = performedByUserId;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedByUserId = performedByUserId;

            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
    performedByUserId,
    AuditActions.UserDeleted,
    EntityNames.User,
    user.Id,
    oldValues: "IsDeleted=false, IsActive=true",
    newValues: "IsDeleted=true, IsActive=false",
    description: $"User {user.Email} was soft deleted."
);

            return true;
        }
        public async Task<bool> UpdateUserRoleAsync(
      Guid id,
      UpdateUserRoleRequest request,
      Guid performedByUserId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return false;
            }

            if (user.IsDeleted)
            {
                throw new InvalidOperationException("Cannot update role for a deleted user.");
            }

            var requestedRole = request.Role.Trim();

            if (await IsLastActiveAdminAsync(user.Id) &&
                !requestedRole.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot change the role of the last active admin account.");
            }

            var allowedRoles = new List<string>
    {
        RoleNames.Admin,
        RoleNames.Seller,
        RoleNames.Customer
    };

            if (!allowedRoles.Contains(requestedRole, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid role.");
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == requestedRole.ToLower());

            if (role == null)
            {
                throw new InvalidOperationException("Invalid role.");
            }

            var oldRoleNames = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList();

            var oldRolesText = string.Join(",", oldRoleNames);

            var alreadyHasSameRole =
                oldRoleNames.Count == 1 &&
                oldRoleNames.Any(r => r.Equals(role.Name, StringComparison.OrdinalIgnoreCase));

            if (alreadyHasSameRole)
            {
                return true;
            }

            _context.UserRoles.RemoveRange(user.UserRoles);

            var newUserRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            _context.UserRoles.Add(newUserRole);

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = performedByUserId;

            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                performedByUserId,
                AuditActions.UserRoleChanged,
                EntityNames.User,
                user.Id,
                oldValues: $"Roles={oldRolesText}",
                newValues: $"Roles={role.Name}",
                description: $"User {user.Email} role changed from {oldRolesText} to {role.Name}."
            );

            return true;
        }

        private async Task<bool> IsLastActiveAdminAsync(Guid userId)
        {
            var userIsAdmin = await _context.UserRoles
                .AnyAsync(ur =>
                    ur.UserId == userId &&
                    ur.Role.Name == RoleNames.Admin
                );

            if (!userIsAdmin)
            {
                return false;
            }

            var activeAdminCount = await _context.Users
                .Where(u => u.IsActive && !u.IsDeleted)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Admin))
                .CountAsync();

            return activeAdminCount <= 1;
        }
    }
}