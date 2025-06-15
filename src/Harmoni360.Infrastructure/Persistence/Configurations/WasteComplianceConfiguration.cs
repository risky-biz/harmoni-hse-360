using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WasteComplianceConfiguration : IEntityTypeConfiguration<WasteCompliance>
{
    public void Configure(EntityTypeBuilder<WasteCompliance> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.RegulatoryBody)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.RegulationCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.RegulationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Status)
            .HasConversion<int>();

        builder.ToTable("WasteCompliances");
    }
}
