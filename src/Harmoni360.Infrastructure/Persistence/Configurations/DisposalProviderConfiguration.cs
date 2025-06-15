using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class DisposalProviderConfiguration : IEntityTypeConfiguration<DisposalProvider>
{
    public void Configure(EntityTypeBuilder<DisposalProvider> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Status)
            .HasConversion<int>();

        builder.ToTable("DisposalProviders");
    }
}
