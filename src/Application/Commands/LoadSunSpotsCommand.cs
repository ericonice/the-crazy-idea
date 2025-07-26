using System.Globalization;
using CsvHelper;
using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Earthquakes.Application.Commands;

public record LoadSunSpotsCommand(string SunSpotsFileName) : IRequest;

public class LoadSunSpotsCommandHandler(ILogger<LoadSunSpotsCommand> logger, AppDbContext dbContext)
    : IRequestHandler<LoadSunSpotsCommand>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<LoadSunSpotsCommand> _logger = logger;

    public async Task Handle(LoadSunSpotsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(request.ToString());

        var deletedRows = await _dbContext.SunSpots.ExecuteDeleteAsync(cancellationToken);
        _logger.LogInformation($"Deleted [{deletedRows}] sun spots.");

        var sunSpots = GetSunSpots(request.SunSpotsFileName);

        // Load the data
        await _dbContext.SunSpots.AddRangeAsync(sunSpots, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var numberOfSunSpots = await _dbContext.SunSpots.CountAsync();
        _logger.LogInformation($"Successfully saved [{numberOfSunSpots}] sun spot entries.");
    }

    private IEnumerable<SunSpot> GetSunSpots(string sunSpotFileName)
    {
        var rawSunSpots = GetSunSpotsFromCsv(sunSpotFileName);
        _logger.LogInformation($"Total number of sun spot days in CSV: {rawSunSpots.Count}");

        return rawSunSpots
            .Where(s => s.NumberOfSunSpots >= 0)
            .Select(r => new SunSpot(
                Day: new DateOnly(year: r.Year, month: r.Month, day: r.Day),
                NumberOfSunSpots: r.NumberOfSunSpots,
                NumberOfObservations: r.NumberOfObservations,
                StandardDeviation: r.StandardDeviation,
                Provisional: !r.Definitive
            ));
    }

    private List<RawSunSpot> GetSunSpotsFromCsv(string sunSpotFileName)
    {
        _logger.LogInformation(
            "Reading sun spots from CSV file: {SunSpotFileName}",
            sunSpotFileName
        );

        var records = new List<RawSunSpot>();
        using (var reader = new StreamReader(sunSpotFileName))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            records.AddRange(csv.GetRecords<RawSunSpot>());
        }

        return records;
    }
}
