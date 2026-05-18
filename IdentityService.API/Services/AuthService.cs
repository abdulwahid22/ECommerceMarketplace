using IdentityService.API.Data;
using IdentityService.API.DTOs;
using IdentityService.API.Helpers;
using IdentityService.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
namespace IdentityService.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;

        public AuthService(
            AppDbContext context,
            IJwtTokenService jwtTokenService,
            IConfiguration configuration)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLower();
            var passwordErrors = PasswordValidator.Validate(request.Password);

            if (passwordErrors.Any())
            {
                throw new InvalidOperationException(string.Join(" ", passwordErrors));
            }
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var requestedRole = request.Role.Trim();

            var allowedPublicRoles = new List<string>
    {
        RoleNames.Customer,
        RoleNames.Seller
    };

            if (!allowedPublicRoles.Contains(requestedRole, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Public registration is only allowed for Customer or Seller accounts.");
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == requestedRole.ToLower());

            if (role == null)
            {
                throw new InvalidOperationException("Invalid role.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            _context.Users.Add(user);
            _context.UserRoles.Add(userRole);

            await _context.SaveChangesAsync();

            var emailVerificationTokenValue = GenerateEmailVerificationToken();
            var emailVerificationTokenExpiresAt = GetEmailVerificationTokenExpiry();

            var emailVerificationToken = new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = emailVerificationTokenValue,
                ExpiresAt = emailVerificationTokenExpiresAt,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailVerificationTokens.Add(emailVerificationToken);
            await _context.SaveChangesAsync();

            var roles = new List<string> { role.Name };

            var expiresAt = GetAccessTokenExpiry();

            var token = _jwtTokenService.GenerateToken(user, roles, expiresAt);

            var refreshTokenValue = GenerateRefreshToken();
            var refreshTokenExpiresAt = GetRefreshTokenExpiry();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = refreshTokenExpiresAt,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles,
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                throw new InvalidOperationException("This account is not active.");
            }
            if (!user.EmailConfirmed)
            {
                throw new InvalidOperationException("Please verify your email before logging in.");
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var roles = user.UserRoles
     .Select(ur => ur.Role.Name)
     .ToList();

            var expiresAt = GetAccessTokenExpiry();

            var token = _jwtTokenService.GenerateToken(user, roles, expiresAt);

            var refreshTokenValue = GenerateRefreshToken();
            var refreshTokenExpiresAt = GetRefreshTokenExpiry();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = refreshTokenExpiresAt,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles,
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        public async Task<AuthResponse> CreateUserAsync(CreateUserRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var passwordErrors = PasswordValidator.Validate(request.Password);

            if (passwordErrors.Any())
            {
                throw new InvalidOperationException(string.Join(" ", passwordErrors));
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            var requestedRole = request.Role.Trim();

            var allowedRoles = new List<string>
    {
        RoleNames.Admin,
        RoleNames.Seller,
        RoleNames.Customer
    };

            if (!allowedRoles.Contains(requestedRole, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid role.");
            }

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == requestedRole.ToLower());

            if (role == null)
            {
                throw new InvalidOperationException("Invalid role.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                IsDeleted = false,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            _context.Users.Add(user);
            _context.UserRoles.Add(userRole);

            await _context.SaveChangesAsync();

            var roles = new List<string> { role.Name };

            var expiresAt = GetAccessTokenExpiry();

            var token = _jwtTokenService.GenerateToken(user, roles, expiresAt);

            var refreshTokenValue = GenerateRefreshToken();
            var refreshTokenExpiresAt = GetRefreshTokenExpiry();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = refreshTokenExpiresAt,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles,
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = refreshTokenValue,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }
        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            if (refreshToken.IsRevoked)
            {
                throw new InvalidOperationException("Refresh token has been revoked.");
            }

            if (refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Refresh token has expired.");
            }

            var user = refreshToken.User;

            if (!user.IsActive || user.IsDeleted)
            {
                throw new InvalidOperationException("This account is not active.");
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            var roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList();

            var expiresAt = GetAccessTokenExpiry();

            var token = _jwtTokenService.GenerateToken(user, roles, expiresAt);

            var newRefreshTokenValue = GenerateRefreshToken();
            var newRefreshTokenExpiresAt = GetRefreshTokenExpiry();

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshTokenValue,
                ExpiresAt = newRefreshTokenExpiresAt,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles,
                Token = token,
                ExpiresAt = expiresAt,
                RefreshToken = newRefreshTokenValue,
                RefreshTokenExpiresAt = newRefreshTokenExpiresAt
            };
        }

        public async Task LogoutAsync(LogoutRequest request)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            if (refreshToken.IsRevoked)
            {
                return;
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async Task VerifyEmailAsync(VerifyEmailRequest request)
        {
            var token = await _context.EmailVerificationTokens
                .Include(evt => evt.User)
                .FirstOrDefaultAsync(evt => evt.Token == request.Token);

            if (token == null)
            {
                throw new InvalidOperationException("Invalid verification token.");
            }

            if (token.IsUsed)
            {
                throw new InvalidOperationException("Verification token has already been used.");
            }

            if (token.ExpiresAt <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Verification token has expired.");
            }

            var user = token.User;

            if (user.EmailConfirmed)
            {
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;

            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ResendEmailVerificationAsync(ResendEmailVerificationRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                throw new InvalidOperationException("This account is not active.");
            }

            if (user.EmailConfirmed)
            {
                throw new InvalidOperationException("Email is already verified.");
            }

            var existingTokens = await _context.EmailVerificationTokens
                .Where(evt => evt.UserId == user.Id && !evt.IsUsed)
                .ToListAsync();

            foreach (var existingToken in existingTokens)
            {
                existingToken.IsUsed = true;
                existingToken.UsedAt = DateTime.UtcNow;
            }

            var emailVerificationTokenValue = GenerateEmailVerificationToken();
            var emailVerificationTokenExpiresAt = GetEmailVerificationTokenExpiry();

            var emailVerificationToken = new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = emailVerificationTokenValue,
                ExpiresAt = emailVerificationTokenExpiresAt,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailVerificationTokens.Add(emailVerificationToken);

            await _context.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            // Security note:
            // We do not reveal whether the email exists or not.
            if (user == null)
            {
                return;
            }

            if (!user.IsActive || user.IsDeleted)
            {
                return;
            }

            var existingTokens = await _context.PasswordResetTokens
                .Where(prt => prt.UserId == user.Id && !prt.IsUsed)
                .ToListAsync();

            foreach (var existingToken in existingTokens)
            {
                existingToken.IsUsed = true;
                existingToken.UsedAt = DateTime.UtcNow;
            }

            var resetTokenValue = GeneratePasswordResetToken();
            var resetTokenExpiresAt = GetPasswordResetTokenExpiry();

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = resetTokenValue,
                ExpiresAt = resetTokenExpiresAt,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);

            await _context.SaveChangesAsync();
        }
        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var passwordErrors = PasswordValidator.Validate(request.NewPassword);

            if (passwordErrors.Any())
            {
                throw new InvalidOperationException(string.Join(" ", passwordErrors));
            }

            var resetToken = await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == request.Token);

            if (resetToken == null)
            {
                throw new InvalidOperationException("Invalid reset token.");
            }

            if (resetToken.IsUsed)
            {
                throw new InvalidOperationException("Reset token has already been used.");
            }

            if (resetToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Reset token has expired.");
            }

            var user = resetToken.User;

            if (!user.IsActive || user.IsDeleted)
            {
                throw new InvalidOperationException("This account is not active.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            resetToken.IsUsed = true;
            resetToken.UsedAt = DateTime.UtcNow;

            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                throw new InvalidOperationException("This account is not active.");
            }

            var currentPasswordValid = BCrypt.Net.BCrypt.Verify(
                request.CurrentPassword,
                user.PasswordHash
            );

            if (!currentPasswordValid)
            {
                throw new InvalidOperationException("Current password is incorrect.");
            }

            var samePassword = BCrypt.Net.BCrypt.Verify(
                request.NewPassword,
                user.PasswordHash
            );

            if (samePassword)
            {
                throw new InvalidOperationException("New password must be different from current password.");
            }

            var passwordErrors = PasswordValidator.Validate(request.NewPassword);

            if (passwordErrors.Any())
            {
                throw new InvalidOperationException(string.Join(" ", passwordErrors));
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            var activeRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
        public async Task<CurrentUserResponse> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (!user.IsActive || user.IsDeleted)
            {
                throw new InvalidOperationException("This account is not active.");
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();

            var newPhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim();

            if (user.PhoneNumber != newPhoneNumber)
            {
                user.PhoneNumber = newPhoneNumber;
                user.PhoneNumberConfirmed = false;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CurrentUserResponse
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = user.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            };
        }
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        private DateTime GetAccessTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:ExpireMinutes"] ?? "60")
            );
        }

        private DateTime GetRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenExpireDays"] ?? "7")
            );
        }

        private string GenerateEmailVerificationToken()
        {
            var randomBytes = new byte[64];

            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        private DateTime GetEmailVerificationTokenExpiry()
        {
            return DateTime.UtcNow.AddHours(
                Convert.ToDouble(_configuration["EmailVerification:ExpireHours"] ?? "24")
            );
        }
        private string GeneratePasswordResetToken()
        {
            var randomBytes = new byte[64];

            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        private DateTime GetPasswordResetTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["PasswordReset:ExpireMinutes"] ?? "30")
            );
        }
    }
}