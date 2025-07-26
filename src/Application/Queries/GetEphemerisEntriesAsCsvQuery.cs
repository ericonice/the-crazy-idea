using System.Globalization;
using CsvHelper;
using Earthquakes.Application.Extensions;
using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Earthquakes.Application.Queries;

public record GetEphemerisEntriesAsCsvQuery(
    Body CenterBody,
    Body TargetBody,
    DateOnly StartOn,
    DateOnly EndOn
) : IRequest;

public class GetEphemerisEntriesAsCsvQueryHandler(
    ILogger<GetEphemerisEntriesAsCsvQuery> logger,
    AppDbContext dbContext,
    IConfiguration configuration
) : IRequestHandler<GetEphemerisEntriesAsCsvQuery>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<GetEphemerisEntriesAsCsvQuery> _logger = logger;

    public async Task Handle(
        GetEphemerisEntriesAsCsvQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(request.ToString());

        using var writer = new StreamWriter(
            _configuration.GetFullPath(
                $"ephemeris_data_{request.StartOn:yyyy-MM-dd}-{request.EndOn:yyyy-MM-dd}-{request.CenterBody}_{request.TargetBody}.csv"
            )
        );
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(
            await _dbContext
                .EphemerisEntries.Where(e =>
                    e.TargetBody == (int)request.TargetBody
                    && e.CenterBody == (int)request.CenterBody
                )
                .OrderBy(s => s.Day)
                .ToArrayAsync(cancellationToken)
        );
    }
}
