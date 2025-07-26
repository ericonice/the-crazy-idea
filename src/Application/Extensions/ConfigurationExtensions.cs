using Microsoft.Extensions.Configuration;

namespace Earthquakes.Application.Extensions;

public static class ConfigurationExtensions
{
    public static string GetFullPath(this IConfiguration configuration, string fileName)
    {
        var outputDirectory = configuration["OutputDirectory"]!;
        return Path.Combine(outputDirectory, fileName);
    }
}
