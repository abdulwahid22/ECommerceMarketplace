using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Reset token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot be longer than 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}