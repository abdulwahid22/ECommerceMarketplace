using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Verification token is required.")]
        public string Token { get; set; } = string.Empty;
    }
}
