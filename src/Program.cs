using CommandLine;
using Earthquakes;
using Earthquakes.Application.Commands;
using Earthquakes.Application.Queries;
using Earthquakes.Domain;
using Earthquakes.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal class Program
{
    public static async Task Main(string[] args)
    {
        // Setup
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<VersionProvider>()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<EarthquakeService>()
            .AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
            })
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly))
            .AddDbContext<AppDbContext>(
                (serviceProvider, options) =>
                {
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    options
                        .UseNpgsql(connectionString)
                        .UseSnakeCaseNamingConvention()
                        .EnableSensitiveDataLogging();
                },
                ServiceLifetime.Transient
            )
            .BuildServiceProvider();

        // Apply migrations
        var db = serviceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var sunSpotsFileName = default(string);
        CommandType commandType = default;
        AlignmentType alignmentType = default;
        Body centerBody = default;
        Body targetBody = default;
        DateOnly startOn = default;
        DateOnly endOn = default;
        decimal minimumMagnitude = default;
        int intervalOffsetStart = default;
        int intervalOffsetEnd = default;
        int minimumInterval = default;
        int maximumInterval = default;

        var parser = new Parser(with =>
        {
            with.CaseInsensitiveEnumValues = true;
            with.HelpWriter = Console.Out;
        })
            .ParseArguments<CommandLineOptions>(args)
            .WithParsed(o =>
            {
                sunSpotsFileName = o.SunSpotsFileName;
                commandType = o.CommandType;
                centerBody = o.CenterBody;
                targetBody = o.TargetBody;
                startOn = o.StartOn;
                endOn = o.EndOn;
                intervalOffsetStart = o.IntervalOffsetStart ?? -30;
                intervalOffsetEnd = o.IntervalOffsetEnd ?? 15;
                minimumMagnitude = o.MinimumMagnitude ?? 7;
                minimumInterval = o.MinimumInterval ?? 5;
                maximumInterval = o.MaximumInterval ?? 60;
                alignmentType = o.Alignment ?? AlignmentType.all;
            })
            .WithNotParsed(errors =>
            {
                Environment.Exit(1);
            });

        switch (commandType)
        {
            case CommandType.LoadEarthquakes:
                await mediator.Send(
                    new LoadEarthquakeCommand(
                        StartOn: startOn,
                        EndOn: endOn,
                        MinimumMagnitude: minimumMagnitude
                    )
                );
                break;

            case CommandType.GetEphemeris:
                await mediator.Send(
                    new GetEphemerisEntriesAsCsvQuery(
                        StartOn: startOn,
                        EndOn: endOn,
                        CenterBody: centerBody,
                        TargetBody: targetBody
                    )
                );
                break;

            case CommandType.LoadEphemeris:
                await mediator.Send(
                    new LoadEphemerisEntriesCommand(
                        StartOn: startOn,
                        EndOn: endOn,
                        CenterBody: centerBody,
                        TargetBody: targetBody
                    )
                );
                break;

            case CommandType.EvaluateChiSquaredForEarthquakeIntervals:
                await mediator.Send(
                    new EvaluateChiSquaredForEarthquakeIntervalsCommand(
                        StartOn: startOn,
                        EndOn: endOn,
                        CenterBody: centerBody,
                        TargetBody: targetBody,
                        MinimumMagnitude: minimumMagnitude,
                        IntervalOffsetStart: intervalOffsetStart,
                        IntervalOffsetEnd: intervalOffsetEnd,
                        MinimumInterval: minimumInterval,
                        MaximumInterval: maximumInterval,
                        AlignmentType: alignmentType
                    )
                );
                break;

            case CommandType.DetermineEarthquakesInInterval:
                await mediator.Send(
                    new DetermineEarthquakesInIntervalCommand(
                        StartOn: startOn,
                        EndOn: endOn,
                        CenterBody: centerBody,
                        TargetBody: targetBody,
                        MinimumMagnitude: minimumMagnitude,
                        IntervalOffsetStart: intervalOffsetStart,
                        IntervalOffsetEnd: intervalOffsetEnd,
                        AlignmentType: alignmentType
                    )
                );
                break;

            case CommandType.GetEarthquakes:
                await mediator.Send(
                    new GetEarthquakesAsCsvQuery(
                        StartOn: startOn,
                        EndOn: endOn,
                        CenterBody: centerBody,
                        MinimumMagnitude: minimumMagnitude
                    )
                );
                break;

            case CommandType.LoadSunSpots:
                await mediator.Send(new LoadSunSpotsCommand(SunSpotsFileName: sunSpotsFileName!));
                break;

            case CommandType.GetSunSpots:
                await mediator.Send(new GetSunSpotsAsCsvQuery());
                break;

            default:
                throw new ArgumentException($"Invalid command type; {commandType}");
        }
    }
}
