using System.ComponentModel;

namespace Earthquakes;

public enum CommandType
{
    [Description("Loads the earthquake data into the database.")]
    LoadEarthquakes,

    [Description("Loads the ephemeris data into the database.")]
    LoadEphemeris,

    [Description("Loads the data from specified sun spots CSV file into the database.")]
    LoadSunSpots,

    [Description("Creates a CSV file for the earthquakes, along with ephemeris data")]
    GetEarthquakes,

    [Description("Creates a CSV file from the existing ephemeris data.")]
    GetEphemeris,

    [Description("Creats a CSV file from the existing sun spots data.")]
    GetSunSpots,

    [Description("Evaluates the Chi Squared for the earthquake intervals.")]
    EvaluateChiSquaredForEarthquakeIntervals,

    [Description("Determines the earthquakes for the given interval")]
    DetermineEarthquakesInInterval,
}
