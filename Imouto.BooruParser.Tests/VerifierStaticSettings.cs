using System.Runtime.CompilerServices;

namespace Imouto.BooruParser.Tests;

public static class VerifierStaticSettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DontScrubDateTimes();
        VerifierSettings.DontScrubGuids();

        UseProjectRelativeDirectory("Verified");
    }
}
