using System.Globalization;
using CsvHelper;
using Earthquakes.Application.Extensions;
using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Earthquakes.Application.Queries;

public record GetEarthquakesAsCsvQuery(
    DateOnly StartOn,
    DateOnly EndOn,
    decimal MinimumMagnitude,
    Body CenterBody
) : IRequest;

public class GetEarthquakesAsCsvQueryHandler(
    AppDbContext dbContext,
    IConfiguration configuration,
    VersionProvider versionProvider
) : IRequestHandler<GetEarthquakesAsCsvQuery>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly VersionProvider _versionProvider = versionProvider;

    public async Task Handle(GetEarthquakesAsCsvQuery request, CancellationToken cancellationToken)
    {
        Console.Out.WriteLine("=================================================================");
        Console.Out.WriteLine("Getting earthquakes.");
        Console.Out.WriteLine($"Start Date            : {request.StartOn}");
        Console.Out.WriteLine($"End Date              : {request.EndOn}");
        Console.Out.WriteLine($"Center Body           : {request.CenterBody}");
        Console.Out.WriteLine($"Minimum Magnitude     : {request.MinimumMagnitude}");
        Console.Out.WriteLine("=================================================================");

        var earthquakes = await _dbContext
            .Earthquakes.Where(e =>
                e.OccurredOn.Date >= request.StartOn.ToDateTime(TimeOnly.MinValue)
                && e.OccurredOn.Date <= request.EndOn.ToDateTime(TimeOnly.MinValue)
                && e.Magnitude >= request.MinimumMagnitude
            )
            .Select(e => new { Earthquake = e, Day = DateOnly.FromDateTime(e.OccurredOn.Date) })
            .ToArrayAsync(cancellationToken);
        Console.Out.WriteLine($"Number of earthquakes in date range: {earthquakes.Length}");

        // Find the ephemeris entries for the days an earthquake occurred
        var ephemerisEntries = await _dbContext
            .EphemerisEntries.Where(eph =>
                eph.CenterBody == (int)request.CenterBody
                && earthquakes.Select(e => e.Day).Contains(eph.Day)
            )
            .ToListAsync(cancellationToken);
        var ephByTargetBodyAndDay = ephemerisEntries
            .GroupBy(e => (e.TargetBody, e.Day))
            .ToDictionary(g => g.Key, g => g.First());

        var earthquakesWithEphemeris = earthquakes
            .Select(e => new
            {
                e.Earthquake.OccurredOn,
                e.Earthquake.Latitude,
                e.Earthquake.Longitude,
                e.Earthquake.Depth,
                e.Earthquake.Magnitude,
                e.Earthquake.MagnitudeType,
                e.Earthquake.MagnitudeError,
                e.Earthquake.MagnitudeSource,
                e.Earthquake.Place,
                VenusSto = ephByTargetBodyAndDay[((int)Body.venus, e.Day)].StoAngle,
                VenusSot = ephByTargetBodyAndDay[((int)Body.venus, e.Day)].SotAngle,
                LunaSto = ephByTargetBodyAndDay[((int)Body.luna, e.Day)].StoAngle,
                LunaSot = ephByTargetBodyAndDay[((int)Body.luna, e.Day)].SotAngle,
                JupiterSto = ephByTargetBodyAndDay[((int)Body.jupiter, e.Day)].StoAngle,
                JupiterSot = ephByTargetBodyAndDay[((int)Body.jupiter, e.Day)].SotAngle
            })
            .ToArray();

        var prefix = $"{_versionProvider.GetVersion()}";
        using var writer = new StreamWriter(
            _configuration.GetFullPath($"{prefix}-earthquakes-with-ephemeris.csv")
        );
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(earthquakesWithEphemeris);
    }
}
