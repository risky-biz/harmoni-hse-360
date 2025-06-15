using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using System.Diagnostics;

namespace Harmoni360.Infrastructure.Services;

public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly ILogger<PerformanceMetricsService> _logger;
    private readonly List<PerformanceMetric> _metrics;
    private readonly object _lock = new();

    public PerformanceMetricsService(ILogger<PerformanceMetricsService> logger)
    {
        _logger = logger;
        _metrics = new List<PerformanceMetric>();
    }

    public Task RecordQueryExecutionTimeAsync(string queryName, TimeSpan executionTime, int resultCount = 0)
    {
        var metric = new PerformanceMetric
        {
            Timestamp = DateTime.UtcNow,
            Category = "Database",
            Operation = queryName,
            ExecutionTime = executionTime,
            ResultCount = resultCount,
            Success = true
        };

        RecordMetric(metric);
        
        if (executionTime.TotalMilliseconds > 1000) // Log slow queries
        {
            _logger.LogWarning("Slow query detected: {QueryName} took {ExecutionTime}ms and returned {ResultCount} results",
                queryName, executionTime.TotalMilliseconds, resultCount);
        }

        return Task.CompletedTask;
    }

    public Task RecordApiRequestTimeAsync(string endpoint, string method, TimeSpan executionTime, int statusCode)
    {
        var metric = new PerformanceMetric
        {
            Timestamp = DateTime.UtcNow,
            Category = "API",
            Operation = $"{method} {endpoint}",
            ExecutionTime = executionTime,
            StatusCode = statusCode,
            Success = statusCode >= 200 && statusCode < 300
        };

        RecordMetric(metric);

        if (executionTime.TotalMilliseconds > 2000) // Log slow API requests
        {
            _logger.LogWarning("Slow API request: {Method} {Endpoint} took {ExecutionTime}ms with status {StatusCode}",
                method, endpoint, executionTime.TotalMilliseconds, statusCode);
        }

        return Task.CompletedTask;
    }

    public Task RecordCacheHitAsync(string cacheKey, string operation)
    {
        var metric = new PerformanceMetric
        {
            Timestamp = DateTime.UtcNow,
            Category = "Cache",
            Operation = $"HIT: {operation}",
            CacheKey = cacheKey,
            ExecutionTime = TimeSpan.Zero,
            Success = true
        };

        RecordMetric(metric);
        _logger.LogDebug("Cache hit for key: {CacheKey} in operation: {Operation}", cacheKey, operation);

        return Task.CompletedTask;
    }

    public Task RecordCacheMissAsync(string cacheKey, string operation)
    {
        var metric = new PerformanceMetric
        {
            Timestamp = DateTime.UtcNow,
            Category = "Cache",
            Operation = $"MISS: {operation}",
            CacheKey = cacheKey,
            ExecutionTime = TimeSpan.Zero,
            Success = false
        };

        RecordMetric(metric);
        _logger.LogDebug("Cache miss for key: {CacheKey} in operation: {Operation}", cacheKey, operation);

        return Task.CompletedTask;
    }

    public Task RecordTrainingOperationAsync(string operation, int trainingId, TimeSpan executionTime, bool success = true)
    {
        var metric = new PerformanceMetric
        {
            Timestamp = DateTime.UtcNow,
            Category = "Training",
            Operation = operation,
            EntityId = trainingId,
            ExecutionTime = executionTime,
            Success = success
        };

        RecordMetric(metric);

        var level = success ? LogLevel.Information : LogLevel.Warning;
        _logger.Log(level, "Training operation {Operation} for ID {TrainingId} took {ExecutionTime}ms - Success: {Success}",
            operation, trainingId, executionTime.TotalMilliseconds, success);

        return Task.CompletedTask;
    }

    public Task<Dictionary<string, object>> GetPerformanceMetricsAsync(DateTime from, DateTime to)
    {
        lock (_lock)
        {
            var filteredMetrics = _metrics.Where(m => m.Timestamp >= from && m.Timestamp <= to).ToList();
            
            var result = new Dictionary<string, object>
            {
                ["TotalOperations"] = filteredMetrics.Count,
                ["AverageExecutionTime"] = filteredMetrics.Any() ? filteredMetrics.Average(m => m.ExecutionTime.TotalMilliseconds) : 0,
                ["SuccessRate"] = filteredMetrics.Any() ? (decimal)filteredMetrics.Count(m => m.Success) / filteredMetrics.Count * 100 : 100,
                ["DatabaseMetrics"] = GetCategoryMetrics(filteredMetrics, "Database"),
                ["ApiMetrics"] = GetCategoryMetrics(filteredMetrics, "API"),
                ["CacheMetrics"] = GetCategoryMetrics(filteredMetrics, "Cache"),
                ["TrainingMetrics"] = GetCategoryMetrics(filteredMetrics, "Training"),
                ["SlowestOperations"] = filteredMetrics
                    .OrderByDescending(m => m.ExecutionTime)
                    .Take(10)
                    .Select(m => new { m.Operation, ExecutionTime = m.ExecutionTime.TotalMilliseconds, m.Timestamp })
                    .ToList()
            };

            return Task.FromResult(result);
        }
    }

    private void RecordMetric(PerformanceMetric metric)
    {
        lock (_lock)
        {
            _metrics.Add(metric);
            
            // Keep only recent metrics to prevent memory issues
            if (_metrics.Count > 10000)
            {
                var cutoff = DateTime.UtcNow.AddHours(-24);
                _metrics.RemoveAll(m => m.Timestamp < cutoff);
            }
        }
    }

    private object GetCategoryMetrics(List<PerformanceMetric> metrics, string category)
    {
        var categoryMetrics = metrics.Where(m => m.Category == category).ToList();
        
        if (!categoryMetrics.Any())
        {
            return new { Count = 0, AverageTime = 0, SuccessRate = 100 };
        }

        var result = new
        {
            Count = categoryMetrics.Count,
            AverageTime = categoryMetrics.Average(m => m.ExecutionTime.TotalMilliseconds),
            SuccessRate = (decimal)categoryMetrics.Count(m => m.Success) / categoryMetrics.Count * 100,
            Operations = categoryMetrics
                .GroupBy(m => m.Operation)
                .Select(g => new
                {
                    Operation = g.Key,
                    Count = g.Count(),
                    AverageTime = g.Average(m => m.ExecutionTime.TotalMilliseconds),
                    SuccessRate = (decimal)g.Count(m => m.Success) / g.Count() * 100
                })
                .OrderByDescending(o => o.Count)
                .Take(5)
                .ToList()
        };

        return result;
    }

    private class PerformanceMetric
    {
        public DateTime Timestamp { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
        public bool Success { get; set; }
        public int? ResultCount { get; set; }
        public int? StatusCode { get; set; }
        public string? CacheKey { get; set; }
        public int? EntityId { get; set; }
    }
}