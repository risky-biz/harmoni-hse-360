using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Harmoni360.Application.Features.HSSE.DTOs;
using Harmoni360.Application.Common.Interfaces;

namespace Harmoni360.Infrastructure.Services;

public class HSSECacheService : IHSSECacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<HSSECacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // Cache expiry times for different data types
    private static readonly TimeSpan DefaultCacheExpiry = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan StatisticsCacheExpiry = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan TrendsCacheExpiry = TimeSpan.FromHours(1);
    private static readonly TimeSpan DashboardCacheExpiry = TimeSpan.FromMinutes(10);

    public HSSECacheService(
        IDistributedCache cache,
        ILogger<HSSECacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions();
            
            // Set expiry based on data type or use provided expiry
            var cacheExpiry = expiry ?? GetDefaultExpiryForType<T>();
            options.SetAbsoluteExpiration(cacheExpiry);

            await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
            
            _logger.LogDebug("Cached value for key: {Key} with expiry: {Expiry}", key, cacheExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cache entry for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Redis pattern removal requires additional implementation
        // For now, we'll implement specific key removal patterns
        try
        {
            var keysToRemove = new List<string>();
            
            // Common HSSE cache key patterns
            if (pattern.Contains("hsse"))
            {
                keysToRemove.AddRange(new[]
                {
                    "hsse:dashboard:*",
                    "hsse:statistics:*",
                    "hsse:trends:*",
                    "hsse:classifications:*",
                    "hsse:unsafe-conditions:*",
                    "hsse:incident-rates:*",
                    "hsse:safety-performance:*",
                    "hsse:responsible-actions:*"
                });
            }

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key, cancellationToken);
            }
            
            _logger.LogDebug("Removed cache entries matching pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
        }
    }

    public string GenerateCacheKey(string prefix, params object[] parameters)
    {
        var keyParts = new List<string> { prefix };
        
        foreach (var param in parameters)
        {
            if (param != null)
            {
                keyParts.Add(param.ToString()?.ToLowerInvariant() ?? "null");
            }
        }
        
        return string.Join(":", keyParts);
    }

    private TimeSpan GetDefaultExpiryForType<T>()
    {
        var typeName = typeof(T).Name.ToLowerInvariant();
        
        return typeName switch
        {
            "hssedashboarddto" => DashboardCacheExpiry,
            "hazardstatisticsdto" => StatisticsCacheExpiry,
            "monthlyhazarddto" => TrendsCacheExpiry,
            "hazardclassificationdto" => TrendsCacheExpiry,
            "unsafeconditiondto" => StatisticsCacheExpiry,
            "incidentfrequencyratedto" => TrendsCacheExpiry,
            "safetyperformancedto" => TrendsCacheExpiry,
            "responsibleactionsummarydto" => StatisticsCacheExpiry,
            _ => DefaultCacheExpiry
        };
    }
}

