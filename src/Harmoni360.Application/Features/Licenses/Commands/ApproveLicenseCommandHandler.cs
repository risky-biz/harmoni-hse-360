using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class ApproveLicenseCommandHandler : IRequestHandler<ApproveLicenseCommand, LicenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ApproveLicenseCommandHandler> _logger;

    public ApproveLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ApproveLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LicenseDto> Handle(ApproveLicenseCommand request, CancellationToken cancellationToken)
    {
        var license = await _context.Licenses
            .Include(l => l.Attachments)
            .Include(l => l.Renewals)
            .Include(l => l.LicenseConditions)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (license == null)
        {
            throw new KeyNotFoundException($"License with ID {request.Id} not found.");
        }

        // Approve the license
        var approvedBy = _currentUserService.Name ?? "System";
        license.Approve(approvedBy, request.ApprovalNotes);
        
        // Update dates if provided
        if (request.EffectiveDate.HasValue || request.ExpiryDate.HasValue)
        {
            // This would require adding an UpdateDates method to the License entity
            // For now, we'll use the existing dates
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "License {LicenseNumber} approved by {ApprovedBy} at {ApprovedAt}",
            license.LicenseNumber,
            approvedBy,
            DateTime.UtcNow);

        return _mapper.Map<LicenseDto>(license);
    }
}