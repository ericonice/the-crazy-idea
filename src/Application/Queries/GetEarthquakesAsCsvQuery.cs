using System.Globalization;
using CsvHelper;
using Earthquakes.Application.Extensions;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Earthquakes.Application.Queries;

public record GetEarthquakesAsCsvQuery() : IRequest;

public class GetEarthquakesAsCsvQueryHandler(AppDbContext dbContext, IConfiguration configuration)
    : IRequestHandler<GetEarthquakesAsCsvQuery>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;

    public Task Handle(GetEarthquakesAsCsvQuery request, CancellationToken cancellationToken)
    {
        using var writer = new StreamWriter(_configuration.GetFullPath("earthquakes.csv"));
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(_dbContext.Earthquakes.OrderBy(e => e.OccurredOn));
        return Task.CompletedTask;
    }
}
