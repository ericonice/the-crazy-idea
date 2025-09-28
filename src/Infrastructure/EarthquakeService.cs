using System.Globalization;
using CsvHelper;
using Earthquakes.Domain;
using Microsoft.Extensions.Logging;

namespace Earthquakes.Infrastructure;

public class EarthquakeService
{
    private const string UsgsBaseUrl = "https://earthquake.usgs.gov/fdsnws/event/1";
    private readonly ILogger<EarthquakeService> _logger;

    public EarthquakeService(ILogger<EarthquakeService> logger)
    {
        _logger = logger;
    }

    public async Task<Earthquake[]> GetEarthquakesAsync(
        DateOnly startOn,
        DateOnly endOn,
        decimal minimumMagnitude
    )
    {
        _logger.LogInformation("Getting earthquakes");

        using var client = new HttpClient();

        // Get the count of earthquakes so we can page through the results
        string countUrl =
            $"{UsgsBaseUrl}/count?starttime={startOn:yyyy-MM-dd}&endtime={endOn:yyyy-MM-dd}&minmagnitude={minimumMagnitude}";
        var countResponse = await client.GetAsync(countUrl);
        countResponse.EnsureSuccessStatusCode();
        var totalEarthquakes = int.Parse(await countResponse.Content.ReadAsStringAsync());
        _logger.LogInformation($"Total earthquakes found: {totalEarthquakes}");

        // Page through the results
        var allEarthquakes = new List<Earthquake>();
        const int limit = 10000;
        for (int offset = 1; offset <= totalEarthquakes; offset += limit)
        {
            _logger.LogInformation($"Getting earthquakes for offset {offset}");
            string dataUrl =
                $"{UsgsBaseUrl}/query.csv?starttime={startOn:yyyy-MM-dd}&endtime={endOn:yyyy-MM-dd}&minmagnitude={minimumMagnitude}&orderby=time-asc&limit={limit}&offset={offset}";

            var dataResponse = await client.GetAsync(dataUrl);
            dataResponse.EnsureSuccessStatusCode();
            var data = await dataResponse.Content.ReadAsStringAsync();

            using var reader = new StringReader(data);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<EarthquakeMap>();
            allEarthquakes.AddRange(csv.GetRecords<Earthquake>());
        }

        return [.. allEarthquakes];
    }
}
