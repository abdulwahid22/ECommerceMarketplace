using System.Security.Claims;

namespace ProductService.API.Helpers
{
    public static class CurrentUserHelper
    {
        public static Guid? GetUserId(ClaimsPrincipal user)
        {
            var userIdValue = user.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                return null;
            }

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : null;
        }

        public static List<string> GetRoles(ClaimsPrincipal user)
        {
            return user.FindAll(ClaimTypes.Role)
                .Select(r => r.Value)
                .ToList();
        }

        public static bool IsInRole(ClaimsPrincipal user, string role)
        {
            return user.IsInRole(role);
        }
    }
}