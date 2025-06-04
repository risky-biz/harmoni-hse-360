using HarmoniHSE360.Domain.Entities;
using HarmoniHSE360.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace HarmoniHSE360.Infrastructure.Persistence.Configurations;

public class EscalationRuleConfiguration : IEntityTypeConfiguration<EscalationRule>
{
    public void Configure(EntityTypeBuilder<EscalationRule> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasDefaultValue(100);

        // Configure JSON columns for complex types
        builder.Property(x => x.TriggerSeverities)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<IncidentSeverity>>(v, (JsonSerializerOptions?)null) ?? new List<IncidentSeverity>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<List<IncidentSeverity>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(x => x.TriggerStatuses)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<IncidentStatus>>(v, (JsonSerializerOptions?)null) ?? new List<IncidentStatus>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<List<IncidentStatus>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(x => x.TriggerDepartments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(x => x.TriggerLocations)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(x => x.TriggerAfterDuration)
            .HasConversion(
                v => v.HasValue ? v.Value.TotalMilliseconds : (double?)null,
                v => v.HasValue ? TimeSpan.FromMilliseconds(v.Value) : (TimeSpan?)null);

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasMany(x => x.Actions)
            .WithOne(x => x.EscalationRule)
            .HasForeignKey(x => x.EscalationRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.CreatedAt);
    }
}

public class EscalationActionConfiguration : IEntityTypeConfiguration<EscalationAction>
{
    public void Configure(EntityTypeBuilder<EscalationAction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Target)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TemplateId)
            .HasMaxLength(100);

        builder.Property(x => x.Parameters)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new Dictionary<string, string>(c)));

        builder.Property(x => x.Channels)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<NotificationChannel>>(v, (JsonSerializerOptions?)null) ?? new List<NotificationChannel>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<List<NotificationChannel>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));

        builder.Property(x => x.Delay)
            .HasConversion(
                v => v.HasValue ? v.Value.TotalMilliseconds : (double?)null,
                v => v.HasValue ? TimeSpan.FromMilliseconds(v.Value) : (TimeSpan?)null);

        // Indexes
        builder.HasIndex(x => x.EscalationRuleId);
        builder.HasIndex(x => x.Type);
    }
}

public class EscalationHistoryConfiguration : IEntityTypeConfiguration<EscalationHistory>
{
    public void Configure(EntityTypeBuilder<EscalationHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RuleName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ActionType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ActionTarget)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ActionDetails)
            .HasMaxLength(2000);

        builder.Property(x => x.IsSuccessful)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.ExecutedAt)
            .IsRequired();

        builder.Property(x => x.ExecutedBy)
            .HasMaxLength(100);

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(x => x.Incident)
            .WithMany()
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.EscalationRule)
            .WithMany()
            .HasForeignKey(x => x.EscalationRuleId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.EscalationRuleId);
        builder.HasIndex(x => x.ExecutedAt);
        builder.HasIndex(x => x.IsSuccessful);
    }
}

public class NotificationHistoryConfiguration : IEntityTypeConfiguration<NotificationHistory>
{
    public void Configure(EntityTypeBuilder<NotificationHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RecipientId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.RecipientType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TemplateId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
            .HasColumnType("json")
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new Dictionary<string, string>(c)));

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(x => x.Incident)
            .WithMany()
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.RecipientId);
        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.IncidentId, x.RecipientId });
    }
}