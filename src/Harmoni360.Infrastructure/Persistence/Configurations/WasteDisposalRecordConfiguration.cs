using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WasteDisposalRecordConfiguration : IEntityTypeConfiguration<WasteDisposalRecord>
{
    public void Configure(EntityTypeBuilder<WasteDisposalRecord> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Unit)
            .HasConversion<int>();

        builder.Property(w => w.Status)
            .HasConversion<int>();

        builder.HasOne(w => w.WasteReport)
            .WithMany()
            .HasForeignKey(w => w.WasteReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.DisposalProvider)
            .WithMany()
            .HasForeignKey(w => w.DisposalProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("WasteDisposalRecords");
    }
}
