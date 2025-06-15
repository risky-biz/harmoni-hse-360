using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WasteTypeConfiguration : IEntityTypeConfiguration<WasteType>
{
    public void Configure(EntityTypeBuilder<WasteType> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Classification)
            .HasConversion<int>();

        builder.ToTable("WasteTypes");
    }
}
