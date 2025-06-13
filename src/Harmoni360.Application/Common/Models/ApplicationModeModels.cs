namespace Harmoni360.Application.Common.Models;

public enum ApplicationEnvironment
{
    Development,
    Demo,
    Staging,
    Production
}

public class ApplicationModeInfo
{
    public bool IsDemoMode { get; set; }
    public ApplicationEnvironment Environment { get; set; }
    public string EnvironmentDisplayName { get; set; } = string.Empty;
    public string BannerMessage { get; set; } = string.Empty;
    public string BannerColor { get; set; } = "info";
    public bool ShowDemoIndicator { get; set; }
    public DemoLimitations Limitations { get; set; } = new();
}

public class DemoModeSettings
{
    public bool ShowDemoBanner { get; set; } = true;
    public bool AllowDataModification { get; set; } = true;
    public bool AllowUserCreation { get; set; } = false;
    public bool AllowDataDeletion { get; set; } = false;
    public bool ShowSampleDataLabels { get; set; } = true;
    public bool AutoResetData { get; set; } = false;
    public int AutoResetIntervalHours { get; set; } = 24;
    public List<string> RestrictedOperations { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public class DemoLimitations
{
    public bool CanCreateUsers { get; set; } = false;
    public bool CanDeleteData { get; set; } = false;
    public bool CanModifySystemSettings { get; set; } = false;
    public bool CanExportData { get; set; } = true;
    public bool CanSendEmails { get; set; } = false;
    public bool CanSendNotifications { get; set; } = true;
    public int MaxIncidentsPerUser { get; set; } = 50;
    public int MaxAttachmentSizeMB { get; set; } = 10;
    public List<string> DisabledFeatures { get; set; } = new();
    public Dictionary<string, int> OperationLimits { get; set; } = new();
}

