using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Application.Features.PPE.DTOs;
using HarmoniHSE360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarmoniHSE360.Application.Features.PPE.Commands;

public class CreatePPESizeCommandHandler : IRequestHandler<CreatePPESizeCommand, PPESizeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreatePPESizeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PPESizeDto> Handle(CreatePPESizeCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        var existingSize = await _context.PPESizes
            .FirstOrDefaultAsync(s => s.Code == request.Code, cancellationToken);

        if (existingSize != null)
        {
            throw new InvalidOperationException($"PPE Size with code '{request.Code}' already exists");
        }

        var size = PPESize.Create(
            request.Name,
            request.Code,
            _currentUserService.Email,
            request.Description,
            request.SortOrder
        );

        _context.PPESizes.Add(size);
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