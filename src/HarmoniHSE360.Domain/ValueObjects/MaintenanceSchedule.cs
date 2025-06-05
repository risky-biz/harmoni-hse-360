using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.ValueObjects;

public class MaintenanceSchedule : ValueObject
{
    public int IntervalDays { get; private set; }
    public DateTime? LastMaintenanceDate { get; private set; }
    public DateTime? NextMaintenanceDate { get; private set; }
    public string? MaintenanceInstructions { get; private set; }

    protected MaintenanceSchedule() { } // For EF Core

    private MaintenanceSchedule(
        int intervalDays,
        DateTime? lastMaintenanceDate,
        string? maintenanceInstructions)
    {
        IntervalDays = intervalDays;
        LastMaintenanceDate = lastMaintenanceDate;
        MaintenanceInstructions = maintenanceInstructions;
        NextMaintenanceDate = CalculateNextMaintenanceDate();
    }

    public static MaintenanceSchedule Create(
        int intervalDays,
        DateTime? lastMaintenanceDate = null,
        string? maintenanceInstructions = null)
    {
        if (intervalDays <= 0)
            throw new ArgumentException("Interval days must be positive", nameof(intervalDays));

        return new MaintenanceSchedule(intervalDays, lastMaintenanceDate, maintenanceInstructions);
    }

    public MaintenanceSchedule UpdateLastMaintenance(DateTime maintenanceDate)
    {
        return new MaintenanceSchedule(IntervalDays, maintenanceDate, MaintenanceInstructions);
    }

    public MaintenanceSchedule UpdateInstructions(string instructions)
    {
        return new MaintenanceSchedule(IntervalDays, LastMaintenanceDate, instructions);
    }

    public bool IsMaintenanceDue => NextMaintenanceDate.HasValue && DateTime.UtcNow >= NextMaintenanceDate.Value;

    public bool IsMaintenanceDueSoon(int daysWarning = 7) => 
        NextMaintenanceDate.HasValue && DateTime.UtcNow.AddDays(daysWarning) >= NextMaintenanceDate.Value;

    public int? DaysUntilMaintenance => NextMaintenanceDate.HasValue
        ? (int)(NextMaintenanceDate.Value - DateTime.UtcNow).TotalDays
        : null;

    private DateTime? CalculateNextMaintenanceDate()
    {
        if (!LastMaintenanceDate.HasValue)
            return null;

        return LastMaintenanceDate.Value.AddDays(IntervalDays);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IntervalDays;
        yield return LastMaintenanceDate ?? DateTime.MinValue;
        yield return MaintenanceInstructions ?? string.Empty;
    }
}