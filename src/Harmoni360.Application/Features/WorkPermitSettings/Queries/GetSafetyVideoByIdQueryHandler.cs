using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WorkPermitSettings.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WorkPermitSettings.Queries;

public class GetSafetyVideoByIdQueryHandler : IRequestHandler<GetSafetyVideoByIdQuery, WorkPermitSafetyVideoDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSafetyVideoByIdQueryHandler> _logger;

    public GetSafetyVideoByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetSafetyVideoByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WorkPermitSafetyVideoDto?> Handle(GetSafetyVideoByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var video = await _context.WorkPermitSafetyVideos
                .FirstOrDefaultAsync(v => v.Id == request.VideoId, cancellationToken);

            if (video == null)
            {
                _logger.LogWarning("Safety video with ID {VideoId} not found", request.VideoId);
                return null;
            }

            return _mapper.Map<WorkPermitSafetyVideoDto>(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving safety video with ID {VideoId}", request.VideoId);
            throw;
        }
    }
}