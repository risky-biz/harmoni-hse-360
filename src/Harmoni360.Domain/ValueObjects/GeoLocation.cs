using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.ValueObjects;

public class GeoLocation : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }

    private GeoLocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees");

        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees");

        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoLocation Create(double latitude, double longitude)
    {
        return new GeoLocation(latitude, longitude);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString()
    {
        return $"{Latitude},{Longitude}";
    }
}