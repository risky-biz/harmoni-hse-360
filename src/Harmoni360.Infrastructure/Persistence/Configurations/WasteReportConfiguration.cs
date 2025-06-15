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
