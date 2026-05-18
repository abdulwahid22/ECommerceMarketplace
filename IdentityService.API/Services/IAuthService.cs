using IdentityService.API.DTOs;

namespace IdentityService.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> CreateUserAsync(CreateUserRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

        Task LogoutAsync(LogoutRequest request);
        Task VerifyEmailAsync(VerifyEmailRequest request);
        Task ResendEmailVerificationAsync(ResendEmailVerificationRequest request);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<CurrentUserResponse> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request);
       
    }
}