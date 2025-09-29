using System.Globalization;
using CsvHelper;
using Earthquakes.Application.Extensions;
using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MathNet.Numerics.Distributions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Earthquakes.Application.Commands;

public record EvaluateChiSquaredForEarthquakeIntervalsCommand(
    DateOnly StartOn,
    DateOnly EndOn,
    Body CenterBody,
    Body TargetBody,
    decimal MinimumMagnitude,
    decimal? MaximumMagnitude,
    int IntervalOffsetStart,
    int IntervalOffsetEnd,
    int MinimumInterval,
    int MaximumInterval,
    AlignmentType AlignmentType
) : IRequest;

public class EvaluateChiSquaredForEarthquakeIntervalsCommandHandler(
    AppDbContext dbContext,
    IConfiguration configuration,
    VersionProvider versionProvider
) : IRequestHandler<EvaluateChiSquaredForEarthquakeIntervalsCommand>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly VersionProvider _versionProvider = versionProvider;

    public async Task Handle(
        EvaluateChiSquaredForEarthquakeIntervalsCommand request,
        CancellationToken cancellationToken
    )
    {
        var startOn = request.StartOn;
        var endOn = request.EndOn;

        Console.Out.WriteLine("=================================================================");
        Console.Out.WriteLine("Evaluating Chi Squared for earthquake intervals");
        Console.Out.WriteLine($"Start Date            : {startOn}");
        Console.Out.WriteLine($"End Date              : {endOn}");
        Console.Out.WriteLine($"Center Body           : {request.CenterBody}");
        Console.Out.WriteLine($"Target Body           : {request.TargetBody}");
        Console.Out.WriteLine($"Minimum Magnitude     : {request.MinimumMagnitude}");
        Console.Out.WriteLine($"Maximum Magnitude     : {request.MaximumMagnitude}");
        Console.Out.WriteLine($"Interval Offset Start : {request.IntervalOffsetStart}");
        Console.Out.WriteLine($"Interval Offset End   : {request.IntervalOffsetEnd}");
        Console.Out.WriteLine($"Minimum Interval      : {request.MinimumInterval}");
        Console.Out.WriteLine($"Maximum Interval      : {request.MaximumInterval}");
        Console.Out.WriteLine($"Alignment Type        : {request.AlignmentType}");
        Console.Out.WriteLine("=================================================================");

        // Need to consider targets outside of the requested data range, as the interval may bleed into the requested range
        var adjustedStartOn = startOn.AddDays(
            -(request.MaximumInterval + request.IntervalOffsetEnd + 1)
        );
        var adjustedEndOn =
            request.IntervalOffsetStart < 0
                ? endOn.AddDays(Math.Abs(request.IntervalOffsetStart) + 1)
                : endOn;

        var targetsQuery = _dbContext.EphemerisEntries.Where(e =>
            e.Day >= adjustedStartOn
            && e.Day <= adjustedEndOn
            && e.TargetBody == (int)request.TargetBody
            && e.CenterBody == (int)request.CenterBody
        );

        targetsQuery = request.AlignmentType switch
        {
            AlignmentType.all => targetsQuery.Where(e => e.Minimum),
            AlignmentType.onside => targetsQuery.Where(e => e.OnsideMinimum),
            AlignmentType.offside => targetsQuery.Where(e => e.OffsideMinimum),
            _ => targetsQuery
        };

        var targets = await targetsQuery.OrderBy(e => e.Day).ToArrayAsync(cancellationToken);

        var numberOfTargets = targets.Length;
        Console.Out.WriteLine($"Number of SOT/STO targets identified: {numberOfTargets}");

        var startOnDateTime = ToDateTimeOffset(startOn);
        var endOnDateTime = ToDateTimeOffset(endOn).AddDays(1);
        var numberOfDays = (endOnDateTime - startOnDateTime).Days - 1;

        var earthquakesQuery = _dbContext.Earthquakes.Where(e =>
            e.OccurredOn >= startOnDateTime
            && e.OccurredOn < endOnDateTime
            && e.Magnitude >= request.MinimumMagnitude
        );
        if (request.MaximumMagnitude.HasValue)
        {
            earthquakesQuery = earthquakesQuery.Where(e =>
                e.Magnitude <= request.MaximumMagnitude.Value
            );
        }

        var earthquakes = await earthquakesQuery
            .OrderBy(e => e.OccurredOn)
            .ToArrayAsync(cancellationToken);
        Console.Out.WriteLine($"Number of earthquakes in date range: {earthquakes.Length}");

        var allEarthquakeDays = earthquakes
            .Select(e => DateOnly.FromDateTime(e.OccurredOn.DateTime))
            .ToArray();
        var distinctEarthquakeDays = allEarthquakeDays.Distinct().ToArray();

        Console.Out.WriteLine(
            $"Number of earthquakes days in date range: {distinctEarthquakeDays.Length}"
        );

        var entries = new List<ChiSquaredEntryForEarthquakeInterval>();
        for (
            var intervalOffsetInDays = request.IntervalOffsetStart;
            intervalOffsetInDays <= request.IntervalOffsetEnd;
            intervalOffsetInDays++
        )
        {
            //Console.WriteLine();
            //Console.WriteLine($"Processing data for offset [{intervalOffsetInDays}].");

            for (
                var intervalDurationInDays = request.MinimumInterval;
                intervalDurationInDays <= request.MaximumInterval;
                intervalDurationInDays++
            )
            {
                var numberOfTargetDays = 0;
                var numberOfIntervalEarthquakes = 0;
                var numberOfIntervalEarthquakeDays = 0;
                var targetDays = new HashSet<DateOnly>();

                foreach (var target in targets)
                {
                    var intervalStartOn = target.Day.AddDays(intervalOffsetInDays);
                    var intervalEndOn = intervalStartOn.AddDays(intervalDurationInDays - 1);

                    // Skip if interval falls before period
                    if (intervalEndOn <= startOn)
                    {
                        continue;
                    }

                    // Skip if interval falls after period
                    if (intervalStartOn >= endOn)
                    {
                        continue;
                    }

                    if (intervalStartOn < startOn)
                    {
                        intervalStartOn = startOn;
                    }

                    if (intervalEndOn > endOn)
                    {
                        intervalEndOn = endOn;
                    }

                    for (
                        var dayNumber = intervalStartOn.DayNumber;
                        dayNumber <= intervalEndOn.DayNumber;
                        dayNumber++
                    )
                    {
                        // Make sure this day has not already been in the target range
                        var day = DateOnly.FromDayNumber(dayNumber: dayNumber);
                        if (targetDays.Contains(day))
                        {
                            throw new Exception(
                                $"Overlapping intervals is not supported.  Day {day} occurs in more than one interval."
                            );
                        }

                        targetDays.Add(day);
                    }

                    // numberOfTargetDays only needed for validation and can likely be removed
                    numberOfTargetDays += intervalEndOn.DayNumber - intervalStartOn.DayNumber + 1;
                    if (numberOfTargetDays != targetDays.Count)
                    {
                        throw new Exception(
                            "Internal Error: Discrepancy in number of target days.  Please contact support."
                        );
                    }

                    numberOfIntervalEarthquakes += allEarthquakeDays
                        .Where(e => e >= intervalStartOn && e <= intervalEndOn)
                        .Count();

                    numberOfIntervalEarthquakeDays += distinctEarthquakeDays
                        .Where(e => e >= intervalStartOn && e <= intervalEndOn)
                        .Count();
                }

                var earthquakeDaysWithinTarget = distinctEarthquakeDays
                    .Where(targetDays.Contains)
                    .ToArray();
                if (earthquakeDaysWithinTarget.Length != numberOfIntervalEarthquakeDays)
                {
                    throw new Exception(
                        "Internal Error: Discrepancy in number of target days with a hit. Please contact support."
                    );
                }

                // Construct the observed data
                var numberOfDaysOutsideTarget = numberOfDays - numberOfTargetDays;
                var hitDaysInTarget = numberOfIntervalEarthquakeDays;
                var hitDaysOutsideTarget =
                    distinctEarthquakeDays.Length - numberOfIntervalEarthquakeDays;
                var missDaysInTarget = numberOfTargetDays - hitDaysInTarget;
                var missDaysOutsideTarget = numberOfDaysOutsideTarget - hitDaysOutsideTarget;
                var observedHitMissData = new HitMissData(
                    HitDaysInTarget: hitDaysInTarget,
                    HitDaysOutsideTarget: hitDaysOutsideTarget,
                    MissDaysInTarget: missDaysInTarget,
                    MissDaysOutsideTarget: missDaysOutsideTarget
                );

                // Construct the predicted data
                var predictedHitDaysInTarget =
                    (decimal)numberOfTargetDays * distinctEarthquakeDays.Length / numberOfDays;
                var predictedHitDaysOutsideTarget =
                    (decimal)numberOfDaysOutsideTarget
                    * distinctEarthquakeDays.Length
                    / numberOfDays;
                var predictedMissDaysInTarget = numberOfTargetDays - predictedHitDaysInTarget;
                var predictedMissDaysOutsideTarget =
                    numberOfDaysOutsideTarget - predictedHitDaysOutsideTarget;
                var percentageOfTargetDays = (decimal)numberOfTargetDays / numberOfDays;
                var predictedHitMissData = new HitMissData(
                    HitDaysInTarget: predictedHitDaysInTarget,
                    HitDaysOutsideTarget: predictedHitDaysOutsideTarget,
                    MissDaysInTarget: predictedMissDaysInTarget,
                    MissDaysOutsideTarget: predictedMissDaysOutsideTarget
                );

                var expectedNumberOfEarthquakeDays =
                    distinctEarthquakeDays.Length * percentageOfTargetDays;
                var expectedNumberOfEarthquakes = earthquakes.Length * percentageOfTargetDays;
                var chiSquare = HitMissData.DetermineChi(
                    observed: observedHitMissData,
                    expected: predictedHitMissData
                );

                entries.Add(
                    new ChiSquaredEntryForEarthquakeInterval(
                        // Starting condition
                        StartDay: startOn,
                        EndDay: endOn,
                        MinimumMagnitude: request.MinimumMagnitude,
                        IntervalOffsetInDays: intervalOffsetInDays,
                        IntervalDurationInDays: intervalDurationInDays,
                        AlignmentType: request.AlignmentType,
                        TotalNumberOfDays: numberOfDays,
                        TotalNumberOfEarthquakes: earthquakes.Length,
                        TotalNumberOfEarthquakeDays: distinctEarthquakeDays.Length,
                        TotalNumberOfTargets: numberOfTargets,
                        DaysInTargetIntervals: numberOfTargetDays,
                        DaysOutsideTargetIntervals: numberOfDays - numberOfTargetDays,
                        NumberOfIntervalEarthquakes: numberOfIntervalEarthquakes,
                        NumberOfIntervalEarthquakeDays: numberOfIntervalEarthquakeDays,
                        PercentageOfTargetDays: Math.Round(percentageOfTargetDays, 5),
                        PercentageOfEarthquakes: Math.Round(
                            (decimal)numberOfIntervalEarthquakes / earthquakes.Length,
                            5
                        ),
                        PercentageOfEarthquakeDays: Math.Round(
                            (decimal)numberOfIntervalEarthquakeDays / distinctEarthquakeDays.Length,
                            5
                        ),
                        ExpectedNumberOfEarthquakes: Math.Round(expectedNumberOfEarthquakes, 3),
                        ExpectedNumberOfEarthquakeDays: Math.Round(
                            expectedNumberOfEarthquakeDays,
                            3
                        ),
                        // ChiSquare data
                        ObservedHDT: observedHitMissData.HitDaysInTarget,
                        ObservedHDOT: observedHitMissData.HitDaysOutsideTarget,
                        ObservedMDT: observedHitMissData.MissDaysInTarget,
                        ObservedMDOT: observedHitMissData.MissDaysOutsideTarget,
                        ExpectedHDT: Math.Round(predictedHitMissData.HitDaysInTarget, 3),
                        ExpectedHDOT: Math.Round(predictedHitMissData.HitDaysOutsideTarget, 3),
                        ExpectedMDT: Math.Round(predictedHitMissData.MissDaysInTarget, 3),
                        ExpectedMDOT: Math.Round(predictedHitMissData.MissDaysOutsideTarget, 3),
                        ChiSquare: Math.Round(chiSquare, 3),
                        PValue: Math.Round(1 - ChiSquared.CDF(1, (double)chiSquare), 3)
                    )
                );
            }
        }

        // CSV the target frequencies
        var prefix =
            $"{_versionProvider.GetVersion()}-{request.TargetBody}-{request.AlignmentType}-({request.IntervalOffsetStart} to {request.IntervalOffsetEnd})-({request.MinimumInterval} to {request.MaximumInterval})";
        using (
            var writer = new StreamWriter(
                _configuration.GetFullPath($"{prefix}_target_frequencies.csv")
            )
        )
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(entries);
        }

        // CSV the targets
        using (var writer = new StreamWriter(_configuration.GetFullPath($"{prefix}_targets.csv")))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(targets);
        }

        // CSV the earthquakes
        using (
            var writer = new StreamWriter(_configuration.GetFullPath($"{prefix}_earthquakes.csv"))
        )
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(earthquakes);
        }
    }

    private static DateTimeOffset ToDateTimeOffset(DateOnly dateOnly)
    {
        return new DateTimeOffset(dateOnly.ToDateTime(new TimeOnly()), TimeSpan.Zero);
    }
}
