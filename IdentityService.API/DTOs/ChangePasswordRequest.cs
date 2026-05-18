using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required.")]
        [MaxLength(100, ErrorMessage = "Current password cannot be longer than 100 characters.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
        [MaxLength(100, ErrorMessage = "New password cannot be longer than 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}