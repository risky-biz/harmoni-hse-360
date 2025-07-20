using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace Harmoni360.Web.Services;

public class ElsaAuthenticationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ElsaAuthenticationProvider> _logger;

    public ElsaAuthenticationProvider(IConfiguration configuration, ILogger<ElsaAuthenticationProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                _logger.LogError("JWT signing key is missing");
                return Task.FromResult<ClaimsPrincipal?>(null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Verify it's a JWT token
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token format");
                return Task.FromResult<ClaimsPrincipal?>(null);
            }

            _logger.LogInformation("Token validated successfully for user: {UserId}", 
                principal.FindFirst("sub")?.Value ?? "Unknown");

            return Task.FromResult<ClaimsPrincipal?>(principal);
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token has expired");
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    public string? ExtractTokenFromRequest(HttpContext context)
    {
        // Try Authorization header first
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        // Try cookie
        if (context.Request.Cookies.TryGetValue("harmoni360_auth", out var cookieToken))
        {
            return cookieToken;
        }

        // Try query parameter (for SignalR and WebSocket connections)
        if (context.Request.Query.TryGetValue("access_token", out var queryToken))
        {
            return queryToken.FirstOrDefault();
        }

        return null;
    }
}