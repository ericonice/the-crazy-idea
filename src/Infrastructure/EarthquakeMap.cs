using CsvHelper.Configuration;
using Earthquakes.Domain;

namespace Earthquakes.Infrastructure;

public class EarthquakeMap : ClassMap<Earthquake>
{
    public EarthquakeMap()
    {
        Map(e => e.Id).Name("id");
        Map(e => e.OccurredOn).Name("time");
        Map(e => e.Depth).Name("depth");
        Map(e => e.Latitude).Name("latitude");
        Map(e => e.Longitude).Name("longitude");
        Map(e => e.Magnitude).Name("mag");
        Map(e => e.MagnitudeSource).Name("magSource");
        Map(e => e.MagnitudeError).Name("magError");
        Map(e => e.MagnitudeType).Name("type");
        Map(e => e.Place).Name("place");
    }
}
