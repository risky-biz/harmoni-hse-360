using FluentAssertions;
using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Application.Features.Statistics.Queries;
using Harmoni360.Application.Tests.Common;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Harmoni360.Application.Tests.Features.Statistics;

public class GetHsseStatisticsQueryHandlerTests : BaseTest
{
    private readonly GetHsseStatisticsQueryHandler _handler;

    public GetHsseStatisticsQueryHandlerTests()
    {
        SeedData();
        _handler = new GetHsseStatisticsQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_NoData_ReturnsZeros()
    {
        var result = await _handler.Handle(new GetHsseStatisticsQuery { HoursWorked = 100000 }, CancellationToken.None);
        result.TotalIncidents.Should().Be(0);
        result.TotalHazards.Should().Be(0);
        result.TotalSecurityIncidents.Should().Be(0);
        result.TotalHealthIncidents.Should().Be(0);
        result.Trir.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithData_ReturnsCounts()
    {
        Context.Incidents.Add(Incident.Create("Test", "desc", Domain.Enums.IncidentSeverity.Minor, DateTime.UtcNow, "loc", "Reporter", "reporter@test.com", "Safety"));
        Context.Hazards.Add(Hazard.Create("Hazard", "desc", null, null, "loc", Domain.Enums.HazardSeverity.Low, 1, "Safety"));
        Context.SecurityIncidents.Add(SecurityIncident.Create(Domain.Enums.SecurityIncidentType.PhysicalSecurityBreach, Domain.Enums.SecurityIncidentCategory.UnauthorizedAccess, "test", "desc", Domain.Enums.SecuritySeverity.Low, DateTime.UtcNow, "loc", 1, "tester"));
        Context.HealthIncidents.Add(HealthIncident.Create(1, Domain.Enums.HealthIncidentType.Injury, Domain.Enums.HealthIncidentSeverity.Minor, "sym", "treat", Domain.Enums.TreatmentLocation.SchoolNurse));
        await Context.SaveChangesAsync();

        var result = await _handler.Handle(new GetHsseStatisticsQuery { HoursWorked = 100000, LostTimeInjuries = 1, DaysLost = 2, CompliantRecords = 8, TotalRecords = 10 }, CancellationToken.None);
        result.TotalIncidents.Should().Be(1);
        result.TotalHazards.Should().Be(1);
        result.TotalSecurityIncidents.Should().Be(1);
        result.TotalHealthIncidents.Should().Be(1);
        result.Trir.Should().BeApproximately(2, 0.01);
        result.Ltifr.Should().BeApproximately(10, 0.01);
        result.SeverityRate.Should().BeApproximately(4, 0.01);
        result.ComplianceRate.Should().Be(80);
    }
}
