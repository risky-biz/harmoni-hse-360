using FluentAssertions;
using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Application.Features.Statistics.Queries;
using Harmoni360.Application.Tests.Common;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Security;
using Harmoni360.Domain.Enums;
using static Harmoni360.Domain.Entities.Incident;
using static Harmoni360.Domain.Entities.Hazard;
using static Harmoni360.Domain.Entities.Security.SecurityIncident;
using static Harmoni360.Domain.Entities.HealthIncident;
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
        Context.Incidents.Add(Incident.Create("Test", "desc", IncidentSeverity.Minor, DateTime.UtcNow, "loc", "Reporter", "reporter@test.com", "Safety"));
        Context.Hazards.Add(Hazard.Create("Hazard", "desc", null, null, "loc", HazardSeverity.Minor, 1, "Safety"));
        Context.SecurityIncidents.Add(SecurityIncident.Create(SecurityIncidentType.DataBreach, SecurityIncidentCategory.DataBreach, "test", "desc", SecuritySeverity.Low, DateTime.UtcNow, "loc", 1, "tester"));
        Context.HealthIncidents.Add(HealthIncident.Create(1, HealthIncidentType.Injury, HealthIncidentSeverity.Minor, "sym", "treat", TreatmentLocation.SchoolNurse));
        await Context.SaveChangesAsync();

        var result = await _handler.Handle(new GetHsseTrendQuery(), CancellationToken.None);
        result.Should().NotBeEmpty();
        var item = result.Last();
        (item.IncidentCount + item.HazardCount + item.SecurityIncidentCount + item.HealthIncidentCount).Should().Be(4);
    }
}
