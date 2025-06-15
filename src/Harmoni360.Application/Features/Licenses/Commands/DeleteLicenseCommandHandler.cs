using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class DeleteLicenseCommandHandler : IRequestHandler<DeleteLicenseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteLicenseCommandHandler> _logger;

    public DeleteLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<DeleteLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DeleteLicenseCommand request, CancellationToken cancellationToken)
    {
        var license = await _context.Licenses
            .Include(l => l.Attachments)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (license == null)
        {
            throw new KeyNotFoundException($"License with ID {request.Id} not found.");
        }

        // Only allow deletion of licenses in Draft status
        if (license.Status != LicenseStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot delete license in {license.Status} status. Only Draft licenses can be deleted.");
        }

        // Remove related attachments
        _context.LicenseAttachments.RemoveRange(license.Attachments);
        
        // Remove the license
        _context.Licenses.Remove(license);

        await _context.SaveChangesAsync(cancellationToken);

        var deletedBy = _currentUserService.Name ?? "System";
        _logger.LogInformation(
            "License {LicenseNumber} deleted by {DeletedBy} at {DeletedAt}",
            license.LicenseNumber,
            deletedBy,
            DateTime.UtcNow);

    }
}