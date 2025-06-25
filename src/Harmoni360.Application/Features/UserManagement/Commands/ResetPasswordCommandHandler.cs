using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPasswordHashService passwordHashService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _passwordHashService = passwordHashService;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resetting password for UserId={UserId} by {CurrentUser}",
            request.UserId, _currentUserService.Email);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        // Hash the new password
        var passwordHash = _passwordHashService.HashPassword(request.NewPassword);
        
        user.ResetPassword(passwordHash, request.RequirePasswordChange);
        await _context.SaveChangesAsync(cancellationToken);

        // Log the activity
        var activityLog = UserActivityLog.Create(
            request.UserId,
            "PasswordReset",
            "Password reset by administrator",
            "User",
            request.UserId
        );
        _context.UserActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password reset successfully for UserId: {UserId}", request.UserId);

        return true;
    }
}