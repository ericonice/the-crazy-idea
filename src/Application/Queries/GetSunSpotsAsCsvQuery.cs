using System.Globalization;
using CsvHelper;
using Earthquakes.Application.Commands;
using Earthquakes.Application.Extensions;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Earthquakes.Application.Queries;

public record GetSunSpotsAsCsvQuery() : IRequest;

public class GetSunSpotsAsCsvQueryHandler(
    ILogger<GetSunSpotsAsCsvQuery> logger,
    AppDbContext dbContext,
    IConfiguration configuration,
    VersionProvider versionProvider
) : IRequestHandler<GetSunSpotsAsCsvQuery>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<GetSunSpotsAsCsvQuery> _logger = logger;
    private readonly VersionProvider _versionProvider = versionProvider;

    public Task Handle(GetSunSpotsAsCsvQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(request.ToString());

        using var writer = new StreamWriter(
            _configuration.GetFullPath($"{_versionProvider.GetVersion()}-sunspots.csv")
        );
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(_dbContext.SunSpots.OrderBy(s => s.Day));
        return Task.CompletedTask;
    }
}
