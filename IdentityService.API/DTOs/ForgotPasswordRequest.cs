using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(200, ErrorMessage = "Email cannot be longer than 200 characters.")]
        public string Email { get; set; } = string.Empty;
    }
}