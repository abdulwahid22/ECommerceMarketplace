using IdentityService.API.DTOs;
using IdentityService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace IdentityService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);

                return Ok(ApiResponse<AuthResponse>.Ok(
                    result,
                    "Registration completed successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);

                return Ok(ApiResponse<AuthResponse>.Ok(
                    result,
                    "Login completed successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> CreateUser(CreateUserRequest request)
        {
            try
            {
                var result = await _authService.CreateUserAsync(request);

                return Ok(ApiResponse<AuthResponse>.Ok(
                    result,
                    "User created successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
            }
        }
        [Authorize]
        [HttpGet("me")]
        public ActionResult<ApiResponse<CurrentUserResponse>> Me()
        {
            var userIdValue = User.FindFirst("userId")?.Value;
            var firstName = User.FindFirst("firstName")?.Value;
            var lastName = User.FindFirst("lastName")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value
                        ?? User.FindFirst("email")?.Value;

            var roles = User.FindAll(ClaimTypes.Role)
                .Select(r => r.Value)
                .ToList();

            if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(ApiResponse<CurrentUserResponse>.Fail("Invalid token."));
            }

            var response = new CurrentUserResponse
            {
                UserId = userId,
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty,
                Email = email ?? string.Empty,
                Roles = roles
            };

            return Ok(ApiResponse<CurrentUserResponse>.Ok(
                response,
                "Current user loaded successfully."
            ));
        }
        [Authorize]
        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<CurrentUserResponse>>> UpdateMyProfile(UpdateMyProfileRequest request)
        {
            var userIdValue = User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(ApiResponse<CurrentUserResponse>.Fail("Invalid token."));
            }

            try
            {
                var result = await _authService.UpdateMyProfileAsync(userId, request);

                return Ok(ApiResponse<CurrentUserResponse>.Ok(
                    result,
                    "Profile updated successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CurrentUserResponse>.Fail(ex.Message));
            }
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request);

                return Ok(ApiResponse<AuthResponse>.Ok(
                    result,
                    "Token refreshed successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AuthResponse>.Fail(ex.Message));
            }
        }
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout(LogoutRequest request)
        {
            try
            {
                await _authService.LogoutAsync(request);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Logout completed successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("verify-email")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyEmail(VerifyEmailRequest request)
        {
            try
            {
                await _authService.VerifyEmailAsync(request);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Email verified successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("resend-email-verification")]
        public async Task<ActionResult<ApiResponse<object>>> ResendEmailVerification(ResendEmailVerificationRequest request)
        {
            try
            {
                await _authService.ResendEmailVerificationAsync(request);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Email verification token generated successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request);

            return Ok(ApiResponse<object>.Ok(
                null,
                "If the email exists, a password reset link has been generated."
            ));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Password reset successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(ChangePasswordRequest request)
        {
            var userIdValue = User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));
            }

            try
            {
                await _authService.ChangePasswordAsync(userId, request);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Password changed successfully."
                ));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}