using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ERMSystem.Application.DTOs;
using ERMSystem.Application.Interfaces;
using ERMSystem.Domain.Entities;

namespace ERMSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IPatientRepository patientRepository,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _patientRepository = patientRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var userExists = await _userRepository.UsernameExistsAsync(registerDto.Username);
            if (userExists)
                throw new InvalidOperationException($"Username '{registerDto.Username}' is already taken.");

            if (!Array.Exists(AppRole.Internal, r => r == registerDto.Role))
                throw new ArgumentException($"Invalid role '{registerDto.Role}'. Must be Admin, Doctor, or Receptionist.");

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = registerDto.Role
            };

            await _userRepository.AddAsync(user);

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RegisterPatientAsync(PatientRegisterDto registerDto)
        {
            var normalizedUsername = registerDto.Username.Trim();

            if (await _userRepository.UsernameExistsAsync(normalizedUsername))
                throw new InvalidOperationException($"Username '{normalizedUsername}' is already taken.");

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Username = normalizedUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = AppRole.Patient
            };

            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                AppUserId = user.Id,
                FullName = registerDto.FullName.Trim(),
                DateOfBirth = registerDto.DateOfBirth,
                Gender = registerDto.Gender.Trim(),
                Phone = registerDto.Phone.Trim(),
                Address = registerDto.Address.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            try
            {
                await _patientRepository.AddAsync(patient);
            }
            catch
            {
                await _userRepository.DeleteAsync(user);
                throw;
            }

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid username or password.");

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (!TryGetUserId(principal, out var userId))
                throw new UnauthorizedAccessException("Invalid token subject.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            ValidateRefreshToken(user, request.RefreshToken);

            return await BuildAuthResponseAsync(user);
        }

        public async Task LogoutAsync(RefreshTokenRequestDto request)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (!TryGetUserId(principal, out var userId))
                return;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrWhiteSpace(user.RefreshTokenHash))
                return;

            if (ComputeSha256(request.RefreshToken) == user.RefreshTokenHash)
            {
                user.RefreshTokenRevokedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(AppUser user)
        {
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
            var accessToken = GenerateJwtToken(user, expiresAt);
            var refreshToken = GenerateRefreshToken();
            var refreshTtlDays = int.Parse(_configuration["Jwt:RefreshExpiryDays"] ?? "14");

            user.RefreshTokenHash = ComputeSha256(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(refreshTtlDays);
            user.RefreshTokenRevokedAt = null;
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = accessToken,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        private void ValidateRefreshToken(AppUser user, string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken)
                || string.IsNullOrWhiteSpace(user.RefreshTokenHash)
                || user.RefreshTokenExpiresAt == null)
            {
                throw new UnauthorizedAccessException("Refresh token is missing.");
            }

            if (user.RefreshTokenRevokedAt != null)
                throw new UnauthorizedAccessException("Refresh token has been revoked.");

            if (user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired.");

            var refreshHash = ComputeSha256(refreshToken);
            if (!string.Equals(refreshHash, user.RefreshTokenHash, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        private string GenerateJwtToken(AppUser user, DateTime expiresAt)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token.");
            }

            return principal;
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string ComputeSha256(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId)
        {
            var rawUserId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(rawUserId, out userId);
        }
    }
}
