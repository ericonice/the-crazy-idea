using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using Earthquakes.Domain;

namespace Earthquakes.Infrastructure;

public class HorizonsSystemService
{
    private readonly Body _centerBody;
    private readonly DateOnly _endOn;
    private readonly DateOnly _startOn;
    private readonly Body _targetBody;

    public HorizonsSystemService(DateOnly startOn, DateOnly endOn, Body centerBody, Body targetBody)
    {
        _startOn = startOn;
        _endOn = endOn;
        _centerBody = centerBody;
        _targetBody = targetBody;
    }

    public async Task<EphemerisEntry[]> GetEphemerisDataAsync()
    {
        using var client = new HttpClient();
        string url =
            $"https://ssd.jpl.nasa.gov/api/horizons.api?COMMAND='{(int)_targetBody}'&CENTER='{(int)_centerBody}'&START_TIME='{_startOn}'&STOP_TIME='{_endOn}'&STEP_SIZE='1%20d'&format=text&CSV_FORMAT=YES";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseAsString = await response.Content.ReadAsStringAsync();

        var headerPattern = @"\*+\r?\n([^\n]*)?\r?\n\*+\r?\n\$\$SOE";
        var headerMatches = Regex.Matches(responseAsString, headerPattern, RegexOptions.Singleline);
        var headers = headerMatches.First().Groups[1].Value.Trim();
        headers = string.Join(",", headers.Split(',').Select(header => header.Trim()));
        var dataPattern = @"\$\$SOE(.*?)\$\$EOE";
        var dataMatches = Regex.Matches(responseAsString, dataPattern, RegexOptions.Singleline);
        var data = dataMatches.First().Groups[1].Value.Trim();

        // Remove non-date portion of date
        data = Regex.Replace(data, @"00:00", "");

        var dataAsCsv = headers + Environment.NewLine + data;
        EphemerisEntry[] entries;
        using (var reader = new StringReader(dataAsCsv))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<EphemerisEntryMap>();
            entries = csv.GetRecords<EphemerisEntry>().ToArray();
        }

        foreach (var entry in entries)
        {
            entry.TargetBody = (int)_targetBody;
            entry.CenterBody = (int)_centerBody;
        }

        return entries;
    }
}
