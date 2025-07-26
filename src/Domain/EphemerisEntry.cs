namespace Earthquakes.Domain;

public class EphemerisEntry
{
    // Constructor
    public EphemerisEntry(
        DateOnly day,
        int targetBody,
        int centerBody,
        decimal sotAngle,
        decimal stoAngle,
        bool onsideMinimum,
        bool offsideMinimum
    )
    {
        Day = day;
        TargetBody = targetBody;
        CenterBody = centerBody;
        SotAngle = sotAngle;
        StoAngle = stoAngle;
        OnsideMinimum = onsideMinimum;
        OffsideMinimum = offsideMinimum;
        Minimum = onsideMinimum | offsideMinimum;
    }

    // Parameterless constructor (optional, for serialization/deserialization)
    public EphemerisEntry() { }

    public int CenterBody { get; set; }

    public DateOnly Day { get; set; }

    public bool Minimum { get; set; }

    public bool OffsideMinimum { get; set; }

    public bool OnsideMinimum { get; set; }

    public decimal SotAngle { get; set; }

    public bool SotMinimum { get; set; }

    public decimal StoAngle { get; set; }

    public bool StoMinimum { get; set; }

    public int TargetBody { get; set; }
}
