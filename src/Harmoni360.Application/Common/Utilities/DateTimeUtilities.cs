namespace Harmoni360.Application.Common.Utilities;

/// <summary>
/// Utility class for handling DateTime operations, especially for PostgreSQL compatibility
/// </summary>
public static class DateTimeUtilities
{
    /// <summary>
    /// Ensures DateTime is in UTC format for PostgreSQL compatibility
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>DateTime with UTC kind</returns>
    public static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc 
            ? dateTime 
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    /// <summary>
    /// Ensures nullable DateTime is in UTC format for PostgreSQL compatibility
    /// </summary>
    /// <param name="dateTime">The nullable DateTime to convert</param>
    /// <returns>Nullable DateTime with UTC kind</returns>
    public static DateTime? EnsureUtc(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return null;

        return dateTime.Value.Kind == DateTimeKind.Utc 
            ? dateTime 
            : DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
    }

    /// <summary>
    /// Converts a local DateTime to UTC if it's not already UTC
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>DateTime in UTC</returns>
    public static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime
        };
    }

    /// <summary>
    /// Converts a nullable local DateTime to UTC if it's not already UTC
    /// </summary>
    /// <param name="dateTime">The nullable DateTime to convert</param>
    /// <returns>Nullable DateTime in UTC</returns>
    public static DateTime? ToUtc(DateTime? dateTime)
    {
        return dateTime.HasValue ? ToUtc(dateTime.Value) : null;
    }
}