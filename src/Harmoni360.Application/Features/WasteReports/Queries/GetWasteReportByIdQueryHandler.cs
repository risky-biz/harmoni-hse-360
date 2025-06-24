using AutoMapper;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.WasteReports.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.WasteReports.Queries;

public class GetWasteReportByIdQueryHandler : IRequestHandler<GetWasteReportByIdQuery, WasteReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWasteReportByIdQueryHandler> _logger;

    public GetWasteReportByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<GetWasteReportByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<WasteReportDto> Handle(GetWasteReportByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting waste report with ID: {Id}", request.Id);

        var wasteReport = await _context.WasteReports
            .Include(w => w.Attachments)
            .Include(w => w.Reporter)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

        if (wasteReport == null)
        {
            _logger.LogWarning("Waste report with ID {Id} not found", request.Id);
            throw new KeyNotFoundException($"Waste report with ID {request.Id} not found.");
        }

        var dto = _mapper.Map<WasteReportDto>(wasteReport);
        
        _logger.LogInformation("Successfully retrieved waste report with ID: {Id}", request.Id);
        return dto;
    }
}