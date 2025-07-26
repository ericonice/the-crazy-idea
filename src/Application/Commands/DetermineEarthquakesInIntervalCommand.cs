using System.Globalization;
using CsvHelper;
using Earthquakes.Application.Extensions;
using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Earthquakes.Application.Commands;

public record DetermineEarthquakesInIntervalCommand(
    DateOnly StartOn,
    DateOnly EndOn,
    Body CenterBody,
    Body TargetBody,
    decimal MinimumMagnitude,
    int IntervalOffsetStart,
    int IntervalOffsetEnd,
    AlignmentType AlignmentType
) : IRequest;

public class DetermineEarthquakesInIntervalCommandHandler(
    AppDbContext dbContext,
    IConfiguration configuration,
    VersionProvider versionProvider
) : IRequestHandler<DetermineEarthquakesInIntervalCommand>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly VersionProvider _versionProvider = versionProvider;

    public async Task Handle(
        DetermineEarthquakesInIntervalCommand request,
        CancellationToken cancellationToken
    )
    {
        var startOn = request.StartOn;
        var endOn = request.EndOn;

        Console.Out.WriteLine("=================================================================");
        Console.Out.WriteLine("Determining earthquakes in interval");
        Console.Out.WriteLine($"Start Date            : {startOn}");
        Console.Out.WriteLine($"End Date              : {endOn}");
        Console.Out.WriteLine($"Center Body           : {request.CenterBody}");
        Console.Out.WriteLine($"Target Body           : {request.TargetBody}");
        Console.Out.WriteLine($"Minimum Magnitude     : {request.MinimumMagnitude}");
        Console.Out.WriteLine($"Interval Offset Start : {request.IntervalOffsetStart}");
        Console.Out.WriteLine($"Interval Offset End   : {request.IntervalOffsetEnd}");
        Console.Out.WriteLine($"Alignment Type        : {request.AlignmentType}");
        Console.Out.WriteLine("=================================================================");

        var targetsQuery = _dbContext.EphemerisEntries.Where(e =>
            e.Day >= startOn
            && e.Day <= endOn
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
        Console.Out.WriteLine($"Number of targets identified: {numberOfTargets}");

        var startOnDateTime = ToDateTimeOffset(startOn);
        var endOnDateTime = ToDateTimeOffset(endOn).AddDays(1);
        var numberOfDays = (endOnDateTime - startOnDateTime).Days - 1;
        var earthquakes = await _dbContext
            .Earthquakes.Where(e =>
                e.OccurredOn >= startOnDateTime
                && e.OccurredOn < endOnDateTime
                && e.Magnitude >= request.MinimumMagnitude
            )
            .OrderBy(e => e.OccurredOn)
            .Select(e => new { Day = DateOnly.FromDateTime(e.OccurredOn.DateTime), Earthquake = e })
            .ToArrayAsync(cancellationToken);
        Console.Out.WriteLine($"Number of earthquakes in date range: {earthquakes.Length}");

        var numberOfTargetDays = 0;
        var numberOfEarthquakesWithinTarget = 0;
        var targetDays = new HashSet<DateOnly>();
        var hits = new List<Earthquake>();
        foreach (var target in targets.Where(t => t.Day >= startOn && t.Day <= endOn))
        {
            var intervalStartOn = target.Day.AddDays(request.IntervalOffsetStart);

            var intervalEndOn = target.Day.AddDays(request.IntervalOffsetEnd);

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
                    $"Internal Error: Discrepancy in number of target days.  Expected: {numberOfTargetDays}, Actual {targetDays.Count}. Please contact support."
                );
            }

            // numberOfEarthquakesWithinTarget only needed for validation and can likely be removed
            numberOfEarthquakesWithinTarget += earthquakes
                .Where(e => e.Day >= intervalStartOn && e.Day <= intervalEndOn)
                .Count();

            var hitsInTarget = earthquakes.Where(e =>
                e.Day >= intervalStartOn && e.Day <= intervalEndOn
            );

            hits.AddRange(hitsInTarget.Select(h => h.Earthquake));
        }

        if (hits.Count != numberOfEarthquakesWithinTarget)
        {
            throw new Exception(
                $"Internal Error: Discrepancy in number of target days.  Expected: {hits.Count}, Actual {numberOfEarthquakesWithinTarget}. Please contact support."
            );
        }

        var modifiedHits = hits.Select(h => new
            {
                h.OccurredOn,
                Day = DateOnly.FromDateTime(h.OccurredOn.Date),
                h.Latitude,
                h.Longitude,
                h.Depth,
                h.Magnitude,
                h.MagnitudeType,
                h.MagnitudeError,
                h.MagnitudeSource,
                h.Place
            })
            .ToArray();

        var ephemerisEntries = await _dbContext
            .EphemerisEntries.Where(eph =>
                eph.CenterBody == (int)request.CenterBody
                && modifiedHits.Select(h => h.Day).Contains(eph.Day)
            )
            .ToListAsync(cancellationToken);
        var ephByTargetBodyAndDay = ephemerisEntries
            .GroupBy(e => (e.TargetBody, e.Day))
            .ToDictionary(g => g.Key, g => g.First());

        var earthquakesWithEphemeris = modifiedHits
            .Select(h => new
            {
                h.OccurredOn,
                h.Latitude,
                h.Longitude,
                h.Depth,
                h.Magnitude,
                h.MagnitudeType,
                h.MagnitudeError,
                h.MagnitudeSource,
                h.Place,
                VenusSto = ephByTargetBodyAndDay[((int)Body.venus, h.Day)].StoAngle,
                VenusSot = ephByTargetBodyAndDay[((int)Body.venus, h.Day)].SotAngle,
                LunaSto = ephByTargetBodyAndDay[((int)Body.luna, h.Day)].StoAngle,
                LunaSot = ephByTargetBodyAndDay[((int)Body.luna, h.Day)].SotAngle,
                JupiterSto = ephByTargetBodyAndDay[((int)Body.jupiter, h.Day)].StoAngle,
                JupiterSot = ephByTargetBodyAndDay[((int)Body.jupiter, h.Day)].SotAngle
            })
            .ToArray();

        // CSV the earthquakes in the interval
        var prefix = $"{_versionProvider.GetVersion()}-{request.TargetBody}";
        using var writer = new StreamWriter(
            _configuration.GetFullPath(
                $"{prefix}-{request.AlignmentType}-({request.IntervalOffsetStart} to {request.IntervalOffsetEnd})_earthquakes.csv"
            )
        );
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(earthquakesWithEphemeris);
    }

    private static DateTimeOffset ToDateTimeOffset(DateOnly dateOnly)
    {
        return new DateTimeOffset(dateOnly.ToDateTime(new TimeOnly()), TimeSpan.Zero);
    }
}
