using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.PPE.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmoni360.Application.Features.PPE.Commands;

public class UpdatePPESizeCommandHandler : IRequestHandler<UpdatePPESizeCommand, PPESizeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePPESizeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PPESizeDto> Handle(UpdatePPESizeCommand request, CancellationToken cancellationToken)
    {
        var size = await _context.PPESizes
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (size == null)
        {
            throw new InvalidOperationException($"PPE Size with ID {request.Id} not found");
        }

        // Check if code already exists for a different size
        var existingSize = await _context.PPESizes
            .FirstOrDefaultAsync(s => s.Code == request.Code && s.Id != request.Id, cancellationToken);

        if (existingSize != null)
        {
            throw new InvalidOperationException($"PPE Size with code '{request.Code}' already exists");
        }

        size.Update(
            request.Name,
            request.Code,
            _currentUserService.Email,
            request.Description,
            request.SortOrder
        );

        await _context.SaveChangesAsync(cancellationToken);

        return new PPESizeDto
        {
            Id = size.Id,
            Name = size.Name,
            Code = size.Code,
            Description = size.Description,
            SortOrder = size.SortOrder,
            IsActive = size.IsActive,
            CreatedAt = size.CreatedAt,
            CreatedBy = size.CreatedBy,
            LastModifiedAt = size.LastModifiedAt,
            LastModifiedBy = size.LastModifiedBy
        };
    }
}