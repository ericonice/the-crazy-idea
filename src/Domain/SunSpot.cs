using System.ComponentModel.DataAnnotations;

namespace Earthquakes.Domain;

public record SunSpot(
    [property: Key] DateOnly Day,
    int NumberOfSunSpots,
    int NumberOfObservations,
    decimal StandardDeviation,
    bool Provisional
);
