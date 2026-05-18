using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class LogoutRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}