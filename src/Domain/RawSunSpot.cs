namespace Earthquakes.Domain;

public record RawSunSpot(
    int Year,
    int Month,
    int Day,
    decimal YearFraction,
    int NumberOfSunSpots,
    int NumberOfObservations,
    decimal StandardDeviation,
    bool Definitive
);
