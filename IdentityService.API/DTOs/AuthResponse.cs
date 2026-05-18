namespace IdentityService.API.DTOs
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}