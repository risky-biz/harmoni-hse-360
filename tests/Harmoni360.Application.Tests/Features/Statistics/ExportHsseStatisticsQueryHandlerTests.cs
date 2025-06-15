using FluentAssertions;
using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Application.Features.Statistics.Queries;
using Harmoni360.Application.Tests.Common;
using Xunit;

namespace Harmoni360.Application.Tests.Features.Statistics;

public class ExportHsseStatisticsQueryHandlerTests : BaseTest
{
    private readonly ExportHsseStatisticsQueryHandler _handler;

    public ExportHsseStatisticsQueryHandlerTests()
    {
        SeedData();
        var mediator = new MediatR.Mediator(_ => Task.FromResult<object?>(new HsseStatisticsDto()));
        _handler = new ExportHsseStatisticsQueryHandler(mediator);
    }

    [Fact]
    public async Task Handle_ReturnsPdfFile()
    {
        var query = new ExportHsseStatisticsQuery
        {
            HoursWorked = 100000,
            LostTimeInjuries = 1,
            DaysLost = 2,
            CompliantRecords = 8,
            TotalRecords = 10
        };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.FileContent.Should().NotBeNull();
        result.FileName.Should().EndWith(".pdf");
    }
}

