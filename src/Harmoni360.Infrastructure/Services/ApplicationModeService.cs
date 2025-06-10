using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;

namespace Harmoni360.Infrastructure.Services;

public class ApplicationModeService : IApplicationModeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationModeService> _logger;
    private readonly ApplicationModeInfo _modeInfo;
    private readonly DemoModeSettings _demoSettings;

    public ApplicationModeService(
        IConfiguration configuration,
        ILogger<ApplicationModeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Initialize mode settings
        _demoSettings = LoadDemoSettings();
        _modeInfo = LoadModeInfo();
        
        _logger.LogInformation("Application Mode Service initialized. Mode: {Mode}, Environment: {Environment}", 
            IsDemoMode ? "Demo" : "Production", Environment);
    }

    public bool IsDemoMode => bool.TryParse(_configuration["Application:DemoMode"], out var demoMode) && demoMode;
    
    public bool IsProductionMode => !IsDemoMode && Environment == ApplicationEnvironment.Production;
    
    public ApplicationEnvironment Environment
    {
        get
        {
            var envName = _configuration["Application:Environment"] ?? "Development";
            return Enum.TryParse<ApplicationEnvironment>(envName, true, out var result) 
                ? result 
                : ApplicationEnvironment.Development;
        }
    }

    public DemoModeSettings DemoSettings => _demoSettings;

    public ApplicationModeInfo GetModeInfo() => _modeInfo;

    public bool ShouldUseDemoData(string moduleName)
    {
        if (!IsDemoMode) return false;
        
        var moduleKey = $"DataSeeding:Seed{moduleName}";
        return bool.TryParse(_configuration[moduleKey], out var shouldSeed) && shouldSeed;
    }

    public DemoLimitations GetDemoLimitations()
    {
        if (!IsDemoMode)
        {
            return new DemoLimitations
            {
                CanCreateUsers = true,
                CanDeleteData = true,
                CanModifySystemSettings = true,
                CanExportData = true,
                CanSendEmails = true,
                CanSendNotifications = true,
                MaxIncidentsPerUser = int.MaxValue,
                MaxAttachmentSizeMB = 100
            };
        }

        return new DemoLimitations
        {
            CanCreateUsers = _demoSettings.AllowUserCreation,
            CanDeleteData = _demoSettings.AllowDataDeletion,
            CanModifySystemSettings = false,
            CanExportData = true,
            CanSendEmails = false,
            CanSendNotifications = true,
            MaxIncidentsPerUser = 50,
            MaxAttachmentSizeMB = 10,
            DisabledFeatures = new List<string>
            {
                "EmailConfiguration",
                "SystemBackup",
                "UserManagement"
            },
            OperationLimits = new Dictionary<string, int>
            {
                { "IncidentsPerHour", 10 },
                { "AttachmentsPerIncident", 5 },
                { "ReportsPerDay", 20 }
            }
        };
    }

    public bool IsOperationAllowed(string operationType)
    {
        if (!IsDemoMode) return true;

        var limitations = GetDemoLimitations();
        
        return operationType switch
        {
            "CreateUser" => limitations.CanCreateUsers,
            "DeleteData" => limitations.CanDeleteData,
            "ModifySystemSettings" => limitations.CanModifySystemSettings,
            "SendEmail" => limitations.CanSendEmails,
            "SendNotification" => limitations.CanSendNotifications,
            "ExportData" => limitations.CanExportData,
            _ => !_demoSettings.RestrictedOperations.Contains(operationType)
        };
    }

    private DemoModeSettings LoadDemoSettings()
    {
        var settings = new DemoModeSettings();
        var section = _configuration.GetSection("Application:DemoSettings");
        
        // Manually bind since Bind extension may not be available
        settings.ShowDemoBanner = bool.TryParse(section["ShowDemoBanner"], out var showBanner) ? showBanner : true;
        settings.AllowDataModification = bool.TryParse(section["AllowDataModification"], out var allowModification) ? allowModification : true;
        settings.AllowUserCreation = bool.TryParse(section["AllowUserCreation"], out var allowUserCreation) && allowUserCreation;
        settings.AllowDataDeletion = bool.TryParse(section["AllowDataDeletion"], out var allowDeletion) && allowDeletion;
        settings.ShowSampleDataLabels = bool.TryParse(section["ShowSampleDataLabels"], out var showLabels) ? showLabels : true;
        
        // Set defaults if not configured
        if (!settings.RestrictedOperations.Any())
        {
            settings.RestrictedOperations = new List<string>
            {
                "DeleteAllData",
                "ResetDatabase",
                "ModifyConfiguration"
            };
        }

        return settings;
    }

    private ApplicationModeInfo LoadModeInfo()
    {
        var info = new ApplicationModeInfo
        {
            IsDemoMode = IsDemoMode,
            Environment = Environment,
            EnvironmentDisplayName = GetEnvironmentDisplayName(),
            ShowDemoIndicator = IsDemoMode,
            Limitations = GetDemoLimitations()
        };

        if (IsDemoMode)
        {
            info.BannerMessage = GetDemoBannerMessage();
            info.BannerColor = "warning";
        }
        else if (Environment == ApplicationEnvironment.Development)
        {
            info.BannerMessage = "Development Environment - Not for production use";
            info.BannerColor = "info";
        }
        else if (Environment == ApplicationEnvironment.Staging)
        {
            info.BannerMessage = "Staging Environment - Testing in progress";
            info.BannerColor = "secondary";
        }

        return info;
    }

    private string GetEnvironmentDisplayName()
    {
        return Environment switch
        {
            ApplicationEnvironment.Development => "Development",
            ApplicationEnvironment.Demo => "Demo",
            ApplicationEnvironment.Staging => "Staging",
            ApplicationEnvironment.Production => "Production",
            _ => "Unknown"
        };
    }

    private string GetDemoBannerMessage()
    {
        var customMessage = _configuration["Application:DemoSettings:BannerMessage"];
        if (!string.IsNullOrEmpty(customMessage))
        {
            return customMessage;
        }

        return Environment switch
        {
            ApplicationEnvironment.Demo => "🎯 Demo Mode - Explore with sample data. Some features are limited.",
            ApplicationEnvironment.Development => "🔧 Demo Mode (Development) - Testing environment with sample data",
            _ => "🔍 Demo Mode Active - This is a demonstration environment"
        };
    }
}