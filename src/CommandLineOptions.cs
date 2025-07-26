using CommandLine;
using Earthquakes.Domain;

namespace Earthquakes;

public class CommandLineOptions
{
    [Option("alignment", Required = false, HelpText = $"Alignment type (All, Onside, Offside)")]
    public AlignmentType? Alignment { get; set; }

    [Option("center-body", Required = false, HelpText = "Center Body")]
    public Body CenterBody { get; set; }

    [Option(
        "command-type",
        Required = true,
        HelpText = $"Type of command to perform (LoadEarthquakes, GetEarthquakes, GetEarthquakesWithEphemeris, LoadEphemeris, GetEphemeris, LoadSunSpots, GetSunSpots, EvaluateChiSquaredForEarthquakeIntervals, DetermineEarthquakesInInterval)"
    )]
    public CommandType CommandType { get; set; }

    [Option("end-date", Required = false, HelpText = "End Date, e.g. 01/30/2010")]
    public DateOnly EndOn { get; set; }

    [Option(
        "interval-offset-end",
        Required = false,
        Default = 15,
        HelpText = "Interval Offset End in days"
    )]
    public int? IntervalOffsetEnd { get; set; }

    [Option(
        "interval-offset-start",
        Required = false,
        Default = -30,
        HelpText = "Interval Offset Start in days"
    )]
    public int? IntervalOffsetStart { get; set; }

    [Option(
        "maximum-interval",
        Required = false,
        Default = 60,
        HelpText = "Maximum interval ind days"
    )]
    public int? MaximumInterval { get; set; }

    [Option(
        "minimum-interval",
        Required = false,
        Default = 5,
        HelpText = "Minimum interval in days"
    )]
    public int? MinimumInterval { get; set; }

    [Option("minimum-magnitude", Required = false, HelpText = "Minimum magnitude")]
    public decimal? MinimumMagnitude { get; set; }

    [Option("start-date", Required = false, HelpText = "Start Date, e.g. 11/14/2007")]
    public DateOnly StartOn { get; set; }

    [Option(
        "sun-spots-filename",
        Required = false,
        HelpText = "The filename that contains the CSV sun spots file."
    )]
    public string SunSpotsFileName { get; set; } = null!;

    [Option("target-body", Required = false, HelpText = "Center Body")]
    public Body TargetBody { get; set; }
}
