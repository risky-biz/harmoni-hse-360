using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WasteReportConfiguration : IEntityTypeConfiguration<WasteReport>
{
    public void Configure(EntityTypeBuilder<WasteReport> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(w => w.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Category)
            .HasConversion<int>();

        builder.Property(w => w.DisposalStatus)
            .HasConversion<int>();

        // Additional fields for comprehensive waste reporting
        builder.Property(w => w.EstimatedQuantity)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.QuantityUnit)
            .HasMaxLength(50);

        builder.Property(w => w.DisposalMethod)
            .HasMaxLength(200);

        builder.Property(w => w.DisposedBy)
            .HasMaxLength(200);

        builder.Property(w => w.DisposalCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.ContractorName)
            .HasMaxLength(200);

        builder.Property(w => w.ManifestNumber)
            .HasMaxLength(100);

        builder.Property(w => w.Treatment)
            .HasMaxLength(500);

        builder.Property(w => w.Notes)
            .HasColumnType("text");

        builder.HasOne(w => w.Reporter)
            .WithMany()
            .HasForeignKey(w => w.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Attachments)
            .WithOne()
            .HasForeignKey(a => a.WasteReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("WasteReports");
    }
}
