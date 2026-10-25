using Financial.Application.Configuration;
using Financial.Application.DTOs;
using Financial.Application.Exceptions;
using Financial.Application.Interfaces;
using Financial.Application.Mappers;
using Financial.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace Financial.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthSettings _authSettings;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings,
        IOptions<AuthSettings> authSettings)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings.Value;
        _authSettings = authSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!_authSettings.AllowRegistration)
        {
            throw new RegistrationDisabledException();
        }

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PreferredCurrency = request.PreferredCurrency
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = GenerateAccessToken(createdUser);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(createdUser.Id, cancellationToken);

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User: UserMapper.ToDto(createdUser)
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        // Get user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            User: UserMapper.ToDto(user)
        );
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        // Get refresh token from database
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        
        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // Revoke old refresh token
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        // Generate new tokens
        var accessToken = GenerateAccessToken(user);
        var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id, cancellationToken);

        return new RefreshTokenResponseDto(
            AccessToken: accessToken,
            RefreshToken: newRefreshToken.Token,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        );
    }

    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token, cancellationToken);
        
        if (refreshToken == null || refreshToken.IsRevoked)
        {
            return; // Token doesn't exist or already revoked
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateAndSaveRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        return await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
