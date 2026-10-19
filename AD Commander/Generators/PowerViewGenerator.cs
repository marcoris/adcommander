using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds PowerView enumeration one-liners (Get-Domain*). Pure text generation only.
// The credential block is optional: PowerView often runs in an already-authenticated context.
public static class PowerViewGenerator
{
    public static string Generate(
        string operation,
        string identity,
        string properties,
        bool verbose,
        string domain,
        string username)
    {
        bool useCredential = domain.Length > 0 && username.Length > 0;
        List<string> lines = new List<string>();

        if (useCredential)
        {
            lines.AddRange(GeneratorHelpers.SecureCredentialBlock(domain, username));
        }

        lines.Add(GeneratorHelpers.PowerViewImport);
        lines.Add("");

        // Only the parameters the user actually filled in are added.
        string command = operation;
        if (identity.Length > 0)
        {
            command += $" -Identity '{GeneratorHelpers.EscapeSingleQuotes(identity)}'";
        }
        if (properties.Length > 0)
        {
            command += $" -Properties {properties}";
        }
        if (useCredential)
        {
            command += " -Credential $Cred";
        }
        if (verbose)
        {
            command += " -Verbose";
        }

        lines.Add(command);

        return string.Join("\r\n", lines);
    }
}
