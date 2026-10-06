using Gulora.Application.Common.Interfaces;
using Gulora.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Gulora.Infrastructure.Services.Auth;

/// <summary>
/// JWT access token ve refresh token üretim/doğrulama servisi.
/// AccessToken 15 dk ömürlü; refresh token değeri bu sınıfta üretilir (DB kaydı Application'da).
/// Anahtarlar yapılandırma üzerinden alınır — repoda sabit string yok (gulora-security.mdc).
/// Token asla loglanmaz.
/// </summary>
internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateAccessToken(User user)
    {
        var signingKey = GetSigningKey();
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var expiry = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue("Jwt:AccessTokenExpiryMinutes", 15));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Guid? GetUserIdFromToken(string accessToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSigningKey(),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false, // Süresi dolmuş tokendan da ID okunabilsin (refresh akışı)
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(accessToken, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwt)
            {
                var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (Guid.TryParse(sub, out var userId))
                    return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Token doğrulama başarısız: {Message}", ex.Message);
            return null;
        }
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var keyValue = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException(
                "Jwt:SigningKey yapılandırılmamış. user-secrets veya ortam değişkeni kullanın.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
    }
}
