using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Earthquakes.Application.Commands;

public record LoadEphemerisEntriesCommand(
    DateOnly StartOn,
    DateOnly EndOn,
    Body CenterBody,
    Body TargetBody
) : IRequest;

public class LoadEphemerisEntriesCommandHandler(
    ILogger<LoadEphemerisEntriesCommand> logger,
    AppDbContext dbContext
) : IRequestHandler<LoadEphemerisEntriesCommand>
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<LoadEphemerisEntriesCommand> _logger = logger;

    public async Task Handle(
        LoadEphemerisEntriesCommand request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(request.ToString());

        var deletedRows = await _dbContext
            .EphemerisEntries.Where(e =>
                e.CenterBody == (int)request.CenterBody && e.TargetBody == (int)request.TargetBody
            )
            .ExecuteDeleteAsync(cancellationToken);
        _logger.LogInformation($"Deleted [{deletedRows}] entries.");

        // Retrieve the data
        var service = new HorizonsSystemService(
            startOn: request.StartOn,
            endOn: request.EndOn,
            centerBody: request.CenterBody,
            targetBody: request.TargetBody
        );
        var entries = await service.GetEphemerisDataAsync();
        _logger.LogInformation($"Horizon system returned: [{entries.Count()}] entries.");

        // Load the data
        await _dbContext.EphemerisEntries.AddRangeAsync(entries);
        await _dbContext.SaveChangesAsync();
        var numberOfEntries = await _dbContext.EphemerisEntries.CountAsync();
        _logger.LogInformation($"Successfully saved [{numberOfEntries}] ephemeris entries.");

        // Mark the minimums
        await MarkMinimumEphemerisEntriesAsync(
            centerBody: request.CenterBody,
            targetBody: request.TargetBody
        );
    }

    private async Task MarkMinimumEphemerisEntriesAsync(Body centerBody, Body targetBody)
    {
        var entries = await _dbContext
            .EphemerisEntries.Where(e =>
                e.TargetBody == (int)targetBody && e.CenterBody == (int)centerBody
            )
            .OrderBy(e => e.Day)
            .ToArrayAsync();

        int totalMinimumCount = 0;
        int onsideMinimumCount = 0;
        int offsideMinimumCount = 0;
        for (int i = 1; i < entries.Length - 1; i++)
        {
            var previousEntry = entries[i - 1];
            var entry = entries[i];
            var nextEntry = entries[i + 1];

            if (entry.SotAngle < previousEntry.SotAngle && entry.SotAngle <= nextEntry.SotAngle)
            {
                totalMinimumCount++;
                entry.Minimum = true;
                entry.SotMinimum = true;

                if (targetBody.InsideEarth() && entry.StoAngle > 90)
                {
                    entry.OnsideMinimum = true;
                    onsideMinimumCount++;
                }
                else if (targetBody.OutsideEarth())
                {
                    entry.OffsideMinimum = true;
                    offsideMinimumCount++;
                }
            }

            if (entry.StoAngle < previousEntry.StoAngle && entry.StoAngle <= nextEntry.StoAngle)
            {
                totalMinimumCount++;
                entry.Minimum = true;
                entry.StoMinimum = true;

                if (targetBody.InsideEarth())
                {
                    offsideMinimumCount++;
                    entry.OffsideMinimum = true;
                }
                else if (targetBody.OutsideEarth() && entry.SotAngle > 90)
                {
                    onsideMinimumCount++;
                    entry.OnsideMinimum = true;
                }
            }
        }

        _logger.LogInformation($"Number of onside minimums: {onsideMinimumCount}");
        _logger.LogInformation($"Number of offside minimums: {offsideMinimumCount}");
        _logger.LogInformation($"Number of total minimums: {totalMinimumCount}");
        await _dbContext.SaveChangesAsync();
    }
}
