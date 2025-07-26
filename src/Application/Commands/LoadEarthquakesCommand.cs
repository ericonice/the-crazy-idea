using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Earthquakes.Application.Commands;

public record LoadEarthquakeCommand(DateOnly StartOn, DateOnly EndOn, decimal MinimumMagnitude)
    : IRequest;

public class LoadEarthquakeCommandHandler(
    ILogger<LoadEarthquakeCommand> logger,
    AppDbContext dbContext
) : IRequestHandler<LoadEarthquakeCommand>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<LoadEarthquakeCommand> _logger = logger;

    public async Task Handle(LoadEarthquakeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(request.ToString());

        var deletedRows = await _dbContext.Earthquakes.ExecuteDeleteAsync(cancellationToken);
        _logger.LogInformation($"Deleted [{deletedRows}] earthquakes.");

        var service = new EarthquakeService(
            startOn: request.StartOn,
            endOn: request.EndOn,
            minimumMagnitude: request.MinimumMagnitude
        );
        var earthquakes = await service.GetEarthquakesAsync();
        _logger.LogInformation($"Earthquake service returned [{earthquakes.Count()}] earthquakes");

        // Load the earthquake data
        await _dbContext.Earthquakes.AddRangeAsync(earthquakes, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var numberOfEarthquakes = await _dbContext.Earthquakes.CountAsync();
        _logger.LogInformation($"Successfully saved [{numberOfEarthquakes}] earthquakes");
    }
}
