namespace Harmoni360.Application.Common.Interfaces;

public interface IHSSECacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    string GenerateCacheKey(string prefix, params object[] parameters);
}

// Cache key constants for HSSE module
public static class HSSECacheKeys
{
    public const string DashboardPrefix = "hsse:dashboard";
    public const string StatisticsPrefix = "hsse:statistics";
    public const string TrendsPrefix = "hsse:trends";
    public const string ClassificationsPrefix = "hsse:classifications";
    public const string UnsafeConditionsPrefix = "hsse:unsafe-conditions";
    public const string IncidentRatesPrefix = "hsse:incident-rates";
    public const string SafetyPerformancePrefix = "hsse:safety-performance";
    public const string ResponsibleActionsPrefix = "hsse:responsible-actions";
}