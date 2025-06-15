using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WorkPermitApprovalConfiguration : IEntityTypeConfiguration<WorkPermitApproval>
{
    public void Configure(EntityTypeBuilder<WorkPermitApproval> builder)
    {
        // Primary Key
        builder.HasKey(wpa => wpa.Id);

        // Properties
        builder.Property(wpa => wpa.ApprovedByName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wpa => wpa.ApprovalLevel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(wpa => wpa.Comments)
            .HasMaxLength(1000);

        builder.Property(wpa => wpa.Signature)
            .HasMaxLength(500);

        builder.Property(wpa => wpa.K3CertificateNumber)
            .HasMaxLength(100);

        builder.Property(wpa => wpa.AuthorityLevel)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(wpa => wpa.WorkPermit)
            .WithMany(wp => wp.Approvals)
            .HasForeignKey(wpa => wpa.WorkPermitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wpa => wpa.WorkPermitId);
        builder.HasIndex(wpa => wpa.ApprovedById);
        builder.HasIndex(wpa => wpa.ApprovalLevel);
        builder.HasIndex(wpa => wpa.ApprovedAt);
        builder.HasIndex(wpa => wpa.IsApproved);

        // Composite indexes
        builder.HasIndex(wpa => new { wpa.WorkPermitId, wpa.ApprovalLevel });
        builder.HasIndex(wpa => new { wpa.WorkPermitId, wpa.ApprovalOrder });

        // Table Configuration
        builder.ToTable("WorkPermitApprovals");

        // Audit fields
        builder.Property(wpa => wpa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(wpa => wpa.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wpa => wpa.LastModifiedAt);

        builder.Property(wpa => wpa.LastModifiedBy)
            .HasMaxLength(100);
    }
}