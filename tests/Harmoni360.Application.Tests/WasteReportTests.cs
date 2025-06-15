using Harmoni360.Domain.Entities.Waste;
using Xunit;

namespace Harmoni360.Application.Tests;

public class WasteReportTests
{
    [Fact]
    public void Create_SetsMandatoryFields()
    {
        var waste = WasteReport.Create("title", "desc", WasteCategory.NonHazardous, DateTime.UtcNow, "loc", null, "tester@demo");
        Assert.Equal("title", waste.Title);
        Assert.Equal(WasteCategory.NonHazardous, waste.Category);
        Assert.Equal("loc", waste.Location);
    }
}
