using FluentAssertions;
using Harmoni360.Application.Features.Statistics.Utils;
using Xunit;

namespace Harmoni360.Application.Tests.Features.Statistics.Utils;

public class HsseKpiCalculatorTests
{
    [Fact]
    public void CalculateTrir_ReturnsExpectedValue()
    {
        var result = HsseKpiCalculator.CalculateTrir(5, 100000);
        result.Should().BeApproximately(10, 0.01);
    }

    [Fact]
    public void CalculateLtifr_ReturnsExpectedValue()
    {
        var result = HsseKpiCalculator.CalculateLtifr(2, 500000);
        result.Should().BeApproximately(4, 0.01);
    }

    [Fact]
    public void CalculateSeverityRate_ReturnsExpectedValue()
    {
        var result = HsseKpiCalculator.CalculateSeverityRate(3, 150000);
        result.Should().BeApproximately(4, 0.01);
    }

    [Fact]
    public void CalculateComplianceRate_ReturnsExpectedValue()
    {
        var result = HsseKpiCalculator.CalculateComplianceRate(80, 100);
        result.Should().Be(80);
    }
}
