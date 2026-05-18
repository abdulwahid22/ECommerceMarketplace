using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class UpdateMyProfileRequest
    {
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100, ErrorMessage = "First name cannot be longer than 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100, ErrorMessage = "Last name cannot be longer than 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(30, ErrorMessage = "Phone number cannot be longer than 30 characters.")]
        public string? PhoneNumber { get; set; }
    }
}