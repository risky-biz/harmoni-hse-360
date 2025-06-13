namespace Harmoni360.Application.Common.Interfaces;

public interface IIncidentCacheService
{
    Task InvalidateIncidentListCacheAsync();
    Task InvalidateUserIncidentCacheAsync(string userId);
    Task InvalidateAllIncidentCachesAsync();
}

public class IncidentCacheService : IIncidentCacheService
{
    private readonly ICacheService _cache;

    public IncidentCacheService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task InvalidateIncidentListCacheAsync()
    {
        await _cache.RemoveByTagAsync("incidents_list");
    }

    public async Task InvalidateUserIncidentCacheAsync(string userId)
    {
        await _cache.RemoveByTagAsync("user_incidents");
        await _cache.RemoveByTagAsync($"user_{userId}_incidents");
    }

    public async Task InvalidateAllIncidentCachesAsync()
    {
        await _cache.RemoveByTagAsync("incidents_list");
        await _cache.RemoveByTagAsync("user_incidents");
        // Remove all user-specific caches by pattern
        await _cache.RemoveByPatternAsync("user_");
    }
}