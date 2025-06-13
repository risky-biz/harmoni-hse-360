using Harmoni360.Application.Common.Models;

namespace Harmoni360.Application.Common.Interfaces;

public interface IApplicationModeService
{
    bool IsDemoMode { get; }
    bool IsProductionMode { get; }
    ApplicationEnvironment Environment { get; }
    DemoModeSettings DemoSettings { get; }
    
    /// <summary>
    /// Get application mode information for frontend
    /// </summary>
    ApplicationModeInfo GetModeInfo();
    
    /// <summary>
    /// Check if demo data should be used for a specific module
    /// </summary>
    bool ShouldUseDemoData(string moduleName);
    
    /// <summary>
    /// Get demo limitations for the current mode
    /// </summary>
    DemoLimitations GetDemoLimitations();
    
    /// <summary>
    /// Validate if an operation is allowed in current mode
    /// </summary>
    bool IsOperationAllowed(string operationType);
}