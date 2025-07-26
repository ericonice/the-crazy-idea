namespace Earthquakes.Domain;

public enum Body
{
    mercury = 199,
    venus = 299,
    luna = 301,
    earth = 399,
    mars = 499,
    jupiter = 599,
    saturn = 699
}

public static class BodyExtensions
{
    public static bool InsideEarth(this Body body)
    {
        return body switch
        {
            Body.mercury or Body.venus => true,
            _ => false,
        };
    }

    public static bool OutsideEarth(this Body body)
    {
        return body switch
        {
            Body.mercury or Body.venus or Body.luna => false,
            _ => true,
        };
    }
}
