namespace Earthquakes.Domain;

public record HitMissData(
    decimal HitDaysInTarget,
    decimal MissDaysInTarget,
    decimal HitDaysOutsideTarget,
    decimal MissDaysOutsideTarget
)
{
    public static decimal DetermineChi(HitMissData observed, HitMissData expected)
    {
        var chiSquare = GetChiSquare(
            observed: observed.HitDaysInTarget,
            expected: expected.HitDaysInTarget
        );
        chiSquare += GetChiSquare(
            observed: observed.HitDaysOutsideTarget,
            expected: expected.HitDaysOutsideTarget
        );
        chiSquare += GetChiSquare(
            observed: observed.MissDaysInTarget,
            expected: expected.MissDaysInTarget
        );
        chiSquare += GetChiSquare(
            observed: observed.MissDaysOutsideTarget,
            expected: expected.MissDaysOutsideTarget
        );
        return chiSquare;
    }

    private static decimal GetChiSquare(decimal observed, decimal expected)
    {
        if (expected == 0)
        {
            if (observed == 0)
            {
                return 0;
            }

            // Return a very large value when expected 0, but observed is not 0
            return decimal.MaxValue;
        }

        return (observed - expected) * (observed - expected) / expected;
    }
}
