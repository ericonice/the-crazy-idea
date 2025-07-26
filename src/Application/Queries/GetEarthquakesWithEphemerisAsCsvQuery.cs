using System.Globalization;
using CsvHelper;
using Earthquakes.Application.Extensions;
using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Earthquakes.Application.Queries;

public record GetEarthquakesWithEphemerisAsCsvQuery(
    DateOnly StartOn,
    DateOnly EndOn,
    decimal MinimumMagnitude,
    Body CenterBody,
    Body TargetBody
) : IRequest;

public class GetEarthquakesWithEphemerisAsCsvQueryHandler(
    AppDbContext dbContext,
    IConfiguration configuration,
    VersionProvider versionProvider
) : IRequestHandler<GetEarthquakesWithEphemerisAsCsvQuery>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly VersionProvider _versionProvider = versionProvider;

    public async Task Handle(
        GetEarthquakesWithEphemerisAsCsvQuery request,
        CancellationToken cancellationToken
    )
    {
        Console.Out.WriteLine("=================================================================");
        Console.Out.WriteLine("Getting earthquakes with angles.");
        Console.Out.WriteLine($"Start Date            : {request.StartOn}");
        Console.Out.WriteLine($"End Date              : {request.EndOn}");
        Console.Out.WriteLine($"Center Body           : {request.CenterBody}");
        Console.Out.WriteLine($"Target Body           : {request.TargetBody}");
        Console.Out.WriteLine($"Minimum Magnitude     : {request.MinimumMagnitude}");
        Console.Out.WriteLine("=================================================================");

        var earthquakesWithEphemeris = await _dbContext
            .Earthquakes.Where(e =>
                e.OccurredOn.Date >= request.StartOn.ToDateTime(TimeOnly.MinValue)
                && e.OccurredOn.Date <= request.EndOn.ToDateTime(TimeOnly.MinValue)
                && e.Magnitude >= request.MinimumMagnitude
            )
            .Join(
                _dbContext.EphemerisEntries.Where(ephemeris =>
                    ephemeris.TargetBody == (int)request.TargetBody
                    && ephemeris.CenterBody == (int)request.CenterBody
                ),
                earthquake => earthquake.OccurredOn.Date,
                ephemeris => ephemeris.Day.ToDateTime(TimeOnly.MinValue),
                (eq, eph) =>
                    new
                    {
                        eq.OccurredOn,
                        eq.Latitude,
                        eq.Longitude,
                        eq.Depth,
                        eq.Magnitude,
                        eq.MagnitudeType,
                        eq.MagnitudeError,
                        eq.MagnitudeSource,
                        eq.Place,
                        eph.StoAngle,
                        eph.SotAngle
                    }
            )
            .ToArrayAsync(cancellationToken);

        var prefix = $"{_versionProvider.GetVersion()}-{request.TargetBody}";
        using var writer = new StreamWriter(
            _configuration.GetFullPath($"{prefix}-earthquakes.csv")
        );
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(earthquakesWithEphemeris);
    }
}
