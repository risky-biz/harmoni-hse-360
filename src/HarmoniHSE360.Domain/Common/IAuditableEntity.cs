namespace HarmoniHSE360.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    string CreatedBy { get; }
    DateTime? LastModifiedAt { get; }
    string? LastModifiedBy { get; }
}