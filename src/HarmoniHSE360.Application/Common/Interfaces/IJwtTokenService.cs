using HarmoniHSE360.Application.Features.Authentication.DTOs;

namespace HarmoniHSE360.Application.Common.Interfaces;

public interface IJwtTokenService
{
    Task<TokenResult> GenerateTokenAsync(int userId, string email, string name, IList<string> roles);
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    Task<TokenResult> RefreshTokenAsync(string token, string refreshToken);
    Task RevokeTokenAsync(string token);
}

public class TokenResult
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}