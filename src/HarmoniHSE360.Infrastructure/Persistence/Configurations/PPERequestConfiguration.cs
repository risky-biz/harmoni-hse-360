using HarmoniHSE360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class PPERequestConfiguration : IEntityTypeConfiguration<PPERequest>
{
    public void Configure(EntityTypeBuilder<PPERequest> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequestNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Justification)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.Priority)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(r => r.ApprovedBy)
            .HasMaxLength(256);

        builder.Property(r => r.FulfilledBy)
            .HasMaxLength(256);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.LastModifiedBy)
            .HasMaxLength(256);

        // Configure relationships
        builder.HasOne(r => r.Requester)
            .WithMany()
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.FulfilledPPEItem)
            .WithMany()
            .HasForeignKey(r => r.FulfilledPPEItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.RequestItems)
            .WithOne(item => item.Request)
            .HasForeignKey(item => item.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.RequestNumber)
            .IsUnique();

        builder.HasIndex(r => r.RequesterId);
        builder.HasIndex(r => r.CategoryId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Priority);
        builder.HasIndex(r => r.RequestDate);
        builder.HasIndex(r => r.RequiredDate);
        builder.HasIndex(r => new { r.Status, r.Priority });

        // Ignore domain events
        builder.Ignore(r => r.DomainEvents);
    }
}

public class PPERequestItemConfiguration : IEntityTypeConfiguration<PPERequestItem>
{
    public void Configure(EntityTypeBuilder<PPERequestItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Size)
            .HasMaxLength(50);

        builder.Property(i => i.SpecialRequirements)
            .HasMaxLength(1000);

        // Configure relationships
        builder.HasOne(i => i.Request)
            .WithMany(r => r.RequestItems)
            .HasForeignKey(i => i.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.RequestId);

        // Ignore domain events
        builder.Ignore(i => i.DomainEvents);
    }
}