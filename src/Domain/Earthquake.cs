namespace Earthquakes.Domain;

public class Earthquake
{
    public string Id { get; init; } = string.Empty;
    public DateTimeOffset OccurredOn { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string? Depth { get; init; }
    public decimal Magnitude { get; init; }
    public string MagnitudeType { get; init; } = string.Empty;
    public decimal? MagnitudeError { get; init; }
    public string MagnitudeSource { get; init; } = string.Empty;
    public string Place { get; init; } = string.Empty;

    // Parameterless constructor for libraries/tools requiring it
    public Earthquake() { }

    // Constructor for initializing values
    public Earthquake(
        string id,
        DateTimeOffset occurredOn,
        decimal latitude,
        decimal longitude,
        string? depth,
        decimal magnitude,
        string magnitudeType,
        decimal? magnitudeError,
        string magnitudeSource,
        string place)
    {
        Id = id;
        OccurredOn = occurredOn;
        Latitude = latitude;
        Longitude = longitude;
        Depth = depth;
        Magnitude = magnitude;
        MagnitudeType = magnitudeType;
        MagnitudeError = magnitudeError;
        MagnitudeSource = magnitudeSource;
        Place = place;
    }
}

