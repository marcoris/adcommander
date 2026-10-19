using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds a SharpHound.exe command line (BloodHound CE collector). Pure text generation only:
// nothing is executed. Flag names and collection methods taken from the real SharpHound Options.
public static class SharpHoundGenerator
{
    public static string Generate(
        List<string> collectionMethods,
        string domain,
        string domainController,
        string outputDirectory,
        string outputPrefix,
        bool loop,
        bool stealth,
        bool zip)
    {
        List<string> lines = new List<string>();

        lines.Add("# SharpHound (BloodHound CE collector): https://github.com/SpecterOps/SharpHound");
        lines.Add("");

        string cmd = "SharpHound.exe";

        if (collectionMethods.Count > 0)
        {
            cmd += " --collectionmethods " + string.Join(",", collectionMethods);
        }
        if (domain.Length > 0)
        {
            cmd += $" --domain {domain}";
        }
        if (domainController.Length > 0)
        {
            cmd += $" --domaincontroller {domainController}";
        }
        if (outputDirectory.Length > 0)
        {
            cmd += $" --outputdirectory {Quote(outputDirectory)}";
        }
        if (outputPrefix.Length > 0)
        {
            cmd += $" --outputprefix {Quote(outputPrefix)}";
        }
        if (loop)
        {
            cmd += " --Loop";
        }
        if (stealth)
        {
            cmd += " --stealth";
        }
        if (!zip)
        {
            // SharpHound zips by default; the flag disables it.
            cmd += " --nozip";
        }

        lines.Add(cmd);

        return string.Join("\r\n", lines);
    }

    // Wrap a value in double quotes (for output paths/prefixes that may contain spaces).
    private static string Quote(string value) => "\"" + value + "\"";
}
