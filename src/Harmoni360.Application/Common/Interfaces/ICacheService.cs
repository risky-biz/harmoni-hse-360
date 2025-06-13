using Microsoft.Extensions.Caching.Memory;

namespace Harmoni360.Application.Common.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, params string[] tags) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByTagAsync(string tag);
    Task RemoveByPatternAsync(string pattern);
    string GenerateKey(string prefix, params object[] parameters);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly Dictionary<string, HashSet<string>> _tagToKeys;
    private readonly Dictionary<string, HashSet<string>> _keyToTags;
    private readonly object _lockObject = new();

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
        _tagToKeys = new Dictionary<string, HashSet<string>>();
        _keyToTags = new Dictionary<string, HashSet<string>>();
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        var result = _cache.Get<T>(key);
        return Task.FromResult(result);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, params string[] tags) where T : class
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }
        else
        {
            // Default expiration
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            options.SlidingExpiration = TimeSpan.FromMinutes(2);
        }

        // Set up cache removal callback to clean up our tracking
        options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
        {
            EvictionCallback = (k, v, reason, state) => RemoveFromTracking(k.ToString()!)
        });

        _cache.Set(key, value, options);

        // Track tags for this cache entry
        if (tags.Length > 0)
        {
            lock (_lockObject)
            {
                _keyToTags[key] = new HashSet<string>(tags);

                foreach (var tag in tags)
                {
                    if (!_tagToKeys.ContainsKey(tag))
                    {
                        _tagToKeys[tag] = new HashSet<string>();
                    }
                    _tagToKeys[tag].Add(key);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        RemoveFromTracking(key);
        return Task.CompletedTask;
    }

    public Task RemoveByTagAsync(string tag)
    {
        lock (_lockObject)
        {
            if (_tagToKeys.TryGetValue(tag, out var keys))
            {
                foreach (var key in keys.ToList())
                {
                    _cache.Remove(key);
                    RemoveFromTracking(key);
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        lock (_lockObject)
        {
            var keysToRemove = _keyToTags.Keys
                .Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                RemoveFromTracking(key);
            }
        }
        return Task.CompletedTask;
    }

    public string GenerateKey(string prefix, params object[] parameters)
    {
        var keyParts = new List<string> { prefix };
        keyParts.AddRange(parameters.Select(p => p?.ToString() ?? "null"));
        return string.Join("_", keyParts);
    }

    private void RemoveFromTracking(string key)
    {
        lock (_lockObject)
        {
            if (_keyToTags.TryGetValue(key, out var tags))
            {
                foreach (var tag in tags)
                {
                    if (_tagToKeys.TryGetValue(tag, out var keySet))
                    {
                        keySet.Remove(key);
                        if (keySet.Count == 0)
                        {
                            _tagToKeys.Remove(tag);
                        }
                    }
                }
                _keyToTags.Remove(key);
            }
        }
    }
}