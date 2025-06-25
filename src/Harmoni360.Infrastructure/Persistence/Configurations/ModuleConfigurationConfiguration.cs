using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ModuleConfiguration entity
/// </summary>
public class ModuleConfigurationConfiguration : IEntityTypeConfiguration<ModuleConfiguration>
{
    public void Configure(EntityTypeBuilder<ModuleConfiguration> builder)
    {
        builder.HasKey(mc => mc.Id);

        // Configure ModuleType enum property
        builder.Property(mc => mc.ModuleType)
            .IsRequired()
            .HasConversion<int>();

        // Configure IsEnabled property
        builder.Property(mc => mc.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Configure DisplayName property
        builder.Property(mc => mc.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        // Configure Description property
        builder.Property(mc => mc.Description)
            .HasMaxLength(500);

        // Configure IconClass property
        builder.Property(mc => mc.IconClass)
            .HasMaxLength(50);

        // Configure DisplayOrder property
        builder.Property(mc => mc.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Configure ParentModuleType enum property
        builder.Property(mc => mc.ParentModuleType)
            .HasConversion<int?>();

        // Configure Settings property as JSON
        builder.Property(mc => mc.Settings)
            .HasColumnType("jsonb"); // PostgreSQL JSONB for better performance

        // Configure audit properties
        builder.Property(mc => mc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(mc => mc.CreatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(mc => mc.LastModifiedAt);

        builder.Property(mc => mc.LastModifiedBy)
            .HasMaxLength(255);

        // Create unique index on ModuleType
        builder.HasIndex(mc => mc.ModuleType)
            .IsUnique()
            .HasDatabaseName("IX_ModuleConfiguration_ModuleType");

        // Create index on IsEnabled for filtering
        builder.HasIndex(mc => mc.IsEnabled)
            .HasDatabaseName("IX_ModuleConfiguration_IsEnabled");

        // Create index on ParentModuleType for hierarchical queries
        builder.HasIndex(mc => mc.ParentModuleType)
            .HasDatabaseName("IX_ModuleConfiguration_ParentModuleType");

        // Create index on DisplayOrder for sorting
        builder.HasIndex(mc => mc.DisplayOrder)
            .HasDatabaseName("IX_ModuleConfiguration_DisplayOrder");

        // Configure self-referencing relationship (Parent-Child)
        builder.HasOne(mc => mc.ParentModule)
            .WithMany(mc => mc.SubModules)
            .HasForeignKey(mc => mc.ParentModuleType)
            .HasPrincipalKey(mc => mc.ModuleType)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure relationship with ModuleDependencies (as the dependent module)
        builder.HasMany(mc => mc.Dependencies)
            .WithOne(md => md.Module)
            .HasForeignKey(md => md.ModuleType)
            .HasPrincipalKey(mc => mc.ModuleType)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with ModuleDependencies (as the dependency)
        builder.HasMany(mc => mc.DependentModules)
            .WithOne(md => md.DependsOnModule)
            .HasForeignKey(md => md.DependsOnModuleType)
            .HasPrincipalKey(mc => mc.ModuleType)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure table name
        builder.ToTable("ModuleConfigurations");
    }
}