using CsvHelper.Configuration;
using Earthquakes.Domain;

namespace Earthquakes.Infrastructure;

public class EphemerisEntryMap : ClassMap<EphemerisEntry>
{
    public EphemerisEntryMap()
    {
        Map(e => e.Day).Name("Date__(UT)__HR:MN");
        Map(e => e.StoAngle).Name("S-T-O");
        Map(e => e.SotAngle).Name("S-O-T");
    }
}
