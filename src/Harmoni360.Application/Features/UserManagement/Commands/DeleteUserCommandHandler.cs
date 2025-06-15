using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user: {UserId} by {CurrentUser}",
            request.UserId, _currentUserService.Email);

        // Prevent self-deletion
        if (request.UserId == _currentUserService.UserId)
        {
            throw new InvalidOperationException("Users cannot deactivate their own account");
        }

        // Get the user to deactivate
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.UserId} not found");
        }

        // Instead of hard delete, we deactivate the user for data integrity
        user.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User deactivated successfully with ID: {UserId}", user.Id);

        return true;
    }
}