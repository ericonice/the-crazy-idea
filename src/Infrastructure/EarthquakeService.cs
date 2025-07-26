using System.Globalization;
using CsvHelper;
using Earthquakes.Domain;

namespace Earthquakes.Infrastructure;

public class EarthquakeService
{
    private readonly DateOnly _endOn;
    private readonly decimal _minimumMagnitude;
    private readonly DateOnly _startOn;

    public EarthquakeService(DateOnly startOn, DateOnly endOn, decimal minimumMagnitude)
    {
        _startOn = startOn;
        _endOn = endOn;
        _minimumMagnitude = minimumMagnitude;
    }

    public async Task<Earthquake[]> GetEarthquakesAsync()
    {
        // Chunk
        return await GetEarthquakesAsync(startOn: _startOn, endOn: _endOn);
    }

    public async Task<Earthquake[]> GetEarthquakesAsync(DateOnly startOn, DateOnly endOn)
    {
        using var client = new HttpClient();
        string url =
            $"https://earthquake.usgs.gov/fdsnws/event/1/query.csv?starttime={startOn}%2000:00:00&endtime={endOn}%2023:59:59&minmagnitude={_minimumMagnitude}&orderby=time-asc";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync();

        Earthquake[] earthquakes;
        using (var reader = new StringReader(data))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<EarthquakeMap>();
            earthquakes = csv.GetRecords<Earthquake>().ToArray();
        }

        return earthquakes;
    }
}
