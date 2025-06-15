using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Licenses.DTOs;

namespace Harmoni360.Application.Features.Licenses.Commands;

public class RevokeLicenseCommandHandler : IRequestHandler<RevokeLicenseCommand, LicenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<RevokeLicenseCommandHandler> _logger;

    public RevokeLicenseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<RevokeLicenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LicenseDto> Handle(RevokeLicenseCommand request, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(request.RevocationReason))
        {
            throw new ArgumentException("Revocation reason is required.");
        }

        var revokedBy = _currentUserService.Name ?? "System";
        license.Revoke(revokedBy, request.RevocationReason);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "License {LicenseNumber} revoked by {RevokedBy} at {RevokedAt}. Reason: {Reason}",
            license.LicenseNumber,
            revokedBy,
            DateTime.UtcNow,
            request.RevocationReason);

        return _mapper.Map<LicenseDto>(license);
    }
}