namespace Harmoni360.Application.Common.Interfaces;

/// <summary>
/// Interface for data seeding classes
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Seeds data asynchronously
    /// </summary>
    /// <returns>A task representing the asynchronous seeding operation</returns>
    Task SeedAsync();
}