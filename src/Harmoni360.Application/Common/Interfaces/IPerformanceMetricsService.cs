namespace Harmoni360.Application.Common.Interfaces;

public interface IPerformanceMetricsService
{
    Task RecordQueryExecutionTimeAsync(string queryName, TimeSpan executionTime, int resultCount = 0);
    Task RecordApiRequestTimeAsync(string endpoint, string method, TimeSpan executionTime, int statusCode);
    Task RecordCacheHitAsync(string cacheKey, string operation);
    Task RecordCacheMissAsync(string cacheKey, string operation);
    Task RecordTrainingOperationAsync(string operation, int trainingId, TimeSpan executionTime, bool success = true);
    Task<Dictionary<string, object>> GetPerformanceMetricsAsync(DateTime from, DateTime to);
}