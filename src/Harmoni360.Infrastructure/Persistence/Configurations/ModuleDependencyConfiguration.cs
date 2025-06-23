using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ModuleDependency entity
/// </summary>
public class ModuleDependencyConfiguration : IEntityTypeConfiguration<ModuleDependency>
{
    public void Configure(EntityTypeBuilder<ModuleDependency> builder)
    {
        builder.HasKey(md => md.Id);

        // Configure ModuleType enum property
        builder.Property(md => md.ModuleType)
            .IsRequired()
            .HasConversion<int>();

        // Configure DependsOnModuleType enum property
        builder.Property(md => md.DependsOnModuleType)
            .IsRequired()
            .HasConversion<int>();

        // Configure IsRequired property
        builder.Property(md => md.IsRequired)
            .IsRequired()
            .HasDefaultValue(true);

        // Configure Description property
        builder.Property(md => md.Description)
            .HasMaxLength(500);

        // Configure audit properties
        builder.Property(md => md.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(md => md.CreatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(md => md.LastModifiedAt);

        builder.Property(md => md.LastModifiedBy)
            .HasMaxLength(255);

        // Create unique index on ModuleType and DependsOnModuleType combination
        builder.HasIndex(md => new { md.ModuleType, md.DependsOnModuleType })
            .IsUnique()
            .HasDatabaseName("IX_ModuleDependency_Module_DependsOn");

        // Create index on ModuleType for better query performance
        builder.HasIndex(md => md.ModuleType)
            .HasDatabaseName("IX_ModuleDependency_ModuleType");

        // Create index on DependsOnModuleType for better query performance
        builder.HasIndex(md => md.DependsOnModuleType)
            .HasDatabaseName("IX_ModuleDependency_DependsOnModuleType");

        // Create index on IsRequired for filtering
        builder.HasIndex(md => md.IsRequired)
            .HasDatabaseName("IX_ModuleDependency_IsRequired");

        // Configure relationships are handled in ModuleConfigurationConfiguration
        // to avoid circular references

        // Configure table name and constraints
        builder.ToTable("ModuleDependencies", t => 
            t.HasCheckConstraint("CK_ModuleDependency_NoSelfDependency", 
                "\"ModuleType\" != \"DependsOnModuleType\""));
    }
}