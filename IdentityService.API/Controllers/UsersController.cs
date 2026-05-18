using IdentityService.API.DTOs;
using IdentityService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<UserResponse>>>> GetUsers(
      [FromQuery] UserQueryParameters query)
        {
            var users = await _userService.GetUsersAsync(query);

            return Ok(ApiResponse<PagedResponse<UserResponse>>.Ok(
                users,
                "Users loaded successfully."
            ));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(ApiResponse<UserResponse>.Fail("User not found."));
            }

            return Ok(ApiResponse<UserResponse>.Ok(
                user,
                "User loaded successfully."
            ));
        }
        [HttpPut("{id:guid}/activate")]
        public async Task<ActionResult<ApiResponse<object>>> ActivateUser(Guid id)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));
            }

            try
            {
                var result = await _userService.ActivateUserAsync(id, currentUserId.Value);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found."));
                }

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "User activated successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("{id:guid}/deactivate")]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(Guid id)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == id)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "You cannot deactivate your own account."
                ));
            }

            try
            {
                if (currentUserId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid token."));
                }

                var result = await _userService.DeactivateUserAsync(id, currentUserId.Value);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found."));
                }

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "User deactivated successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> SoftDeleteUser(Guid id)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == id)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "You cannot delete your own account."
                ));
            }

            try
            {
                if (currentUserId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid token."));
                }

                var result = await _userService.SoftDeleteUserAsync(id, currentUserId.Value);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found."));
                }

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "User deleted successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPut("{id:guid}/role")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUserRole(
    Guid id,
    UpdateUserRoleRequest request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == id)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    "You cannot change your own role."
                ));
            }

            try
            {
                if (currentUserId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Invalid token."));
                }

                var result = await _userService.UpdateUserRoleAsync(id, request, currentUserId.Value);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found."));
                }

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "User role updated successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        private Guid? GetCurrentUserId()
        {
            var userIdValue = User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                return null;
            }

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}