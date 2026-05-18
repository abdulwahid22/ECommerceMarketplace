using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email format is invalid.")]
        [MaxLength(200, ErrorMessage = "Email cannot be longer than 200 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(100, ErrorMessage = "Password cannot be longer than 100 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}