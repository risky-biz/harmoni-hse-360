using Microsoft.AspNetCore.Identity;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Infrastructure.Services;

public class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<object> _passwordHasher;

    public PasswordHashService()
    {
        _passwordHasher = new PasswordHasher<object>();
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(new object(), password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var result = _passwordHasher.VerifyHashedPassword(new object(), hash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}