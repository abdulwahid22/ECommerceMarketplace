using System.ComponentModel.DataAnnotations;

namespace IdentityService.API.DTOs
{
    public class UpdateUserRoleRequest
    {
        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = string.Empty;
    }
}