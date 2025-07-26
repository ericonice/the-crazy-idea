using System.ComponentModel;

namespace Earthquakes;

public enum CommandType
{
    [Description("Loads the earthquake data into the database.")]
    LoadEarthquakes,

    [Description("Create a CSV file from the existing earthquake data.")]
    GetEarthquakes,

    [Description("Loads the ephemeris data into the database.")]
    LoadEphemeris,

    [Description("Create a CSV file from the existing ephemeris data.")]
    GetEphemeris,

    [Description("Loads the data from specified sun spots CSV file into the database.")]
    LoadSunSpots,

    [Description("Create a CSV file from the existing sun spots data.")]
    GetSunSpots,

    [Description("Evaluates the Chi Squared for the earthquake intervals.")]
    EvaluateChiSquaredForEarthquakeIntervals,

    [Description("Determines the earthquakes for the given interval")]
    DetermineEarthquakesInInterval,

    [Description("Gets the earthquakes, along with ephemeris data, for the given interval")]
    GetEarthquakesWithEphemeris
}
