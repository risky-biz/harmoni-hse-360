using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class ActivateLicenseCommandHandler : IRequestHandler<ActivateLicenseCommand, LicenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivateLicenseCommandHandler> _logger;

    public ActivateLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ActivateLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LicenseDto> Handle(ActivateLicenseCommand request, CancellationToken cancellationToken)
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

        var activatedBy = _currentUserService.Name ?? "System";
        license.Activate(activatedBy);

        // Update status notes if provided
        if (!string.IsNullOrWhiteSpace(request.ActivationNotes))
        {
            // This would require adding a method to update status notes
            // For now, we'll just log it
            _logger.LogInformation("Activation notes: {Notes}", request.ActivationNotes);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "License {LicenseNumber} activated by {ActivatedBy} at {ActivatedAt}",
            license.LicenseNumber,
            activatedBy,
            DateTime.UtcNow);

        return _mapper.Map<LicenseDto>(license);
    }
}