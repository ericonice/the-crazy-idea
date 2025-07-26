using System.Reflection;

namespace Earthquakes.Infrastructure;

public class VersionProvider
{
    private readonly string _version;

    public VersionProvider()
    {
        var informationalVersion =
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "unknown";

        _version = informationalVersion.Split('+')[0];
    }

    public string GetVersion() => _version;
}
