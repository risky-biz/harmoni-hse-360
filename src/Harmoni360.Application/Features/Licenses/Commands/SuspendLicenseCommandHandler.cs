using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class SuspendLicenseCommandHandler : IRequestHandler<SuspendLicenseCommand, LicenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<SuspendLicenseCommandHandler> _logger;

    public SuspendLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<SuspendLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LicenseDto> Handle(SuspendLicenseCommand request, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(request.SuspensionReason))
        {
            throw new ArgumentException("Suspension reason is required.");
        }

        var suspendedBy = _currentUserService.Name ?? "System";
        license.Suspend(suspendedBy, request.SuspensionReason);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "License {LicenseNumber} suspended by {SuspendedBy} at {SuspendedAt}. Reason: {Reason}",
            license.LicenseNumber,
            suspendedBy,
            DateTime.UtcNow,
            request.SuspensionReason);

        return _mapper.Map<LicenseDto>(license);
    }
}