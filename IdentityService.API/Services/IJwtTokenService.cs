using IdentityService.API.Models;

namespace IdentityService.API.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, List<string> roles, DateTime expiresAt);
    }
}