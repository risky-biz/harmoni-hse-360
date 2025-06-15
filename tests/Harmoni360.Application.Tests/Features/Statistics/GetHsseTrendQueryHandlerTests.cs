using FluentAssertions;
using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Application.Features.Statistics.Queries;
using Harmoni360.Application.Tests.Common;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Xunit;

namespace Harmoni360.Application.Tests.Features.Statistics;

public class GetHsseTrendQueryHandlerTests : BaseTest
{
    private readonly GetHsseTrendQueryHandler _handler;

    public GetHsseTrendQueryHandlerTests()
    {
        SeedData();
        _handler = new GetHsseTrendQueryHandler(Context);
    }

    [Fact]
    public async Task Handle_NoData_ReturnsEmpty()
    {
        var result = await _handler.Handle(new GetHsseTrendQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithData_ReturnsCounts()
    {
        Context.Incidents.Add(Incident.Create("Test", "desc", Domain.Enums.IncidentSeverity.Minor, DateTime.UtcNow, "loc", "Reporter", "reporter@test.com", "Safety"));
        Context.Hazards.Add(Hazard.Create("Hazard", "desc", null, null, "loc", Domain.Enums.HazardSeverity.Low, 1, "Safety"));
        Context.SecurityIncidents.Add(SecurityIncident.Create(Domain.Enums.SecurityIncidentType.PhysicalSecurityBreach, Domain.Enums.SecurityIncidentCategory.UnauthorizedAccess, "test", "desc", Domain.Enums.SecuritySeverity.Low, DateTime.UtcNow, "loc", 1, "tester"));
        Context.HealthIncidents.Add(HealthIncident.Create(1, Domain.Enums.HealthIncidentType.Injury, Domain.Enums.HealthIncidentSeverity.Minor, "sym", "treat", Domain.Enums.TreatmentLocation.SchoolNurse));
        await Context.SaveChangesAsync();

        var result = await _handler.Handle(new GetHsseTrendQuery(), CancellationToken.None);
        result.Should().NotBeEmpty();
        var item = result.Last();
        (item.IncidentCount + item.HazardCount + item.SecurityIncidentCount + item.HealthIncidentCount).Should().Be(4);
    }
}
