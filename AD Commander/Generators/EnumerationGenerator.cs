using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds broad PowerView situational-awareness commands (domain/trust/GPO/share discovery).
// Pure text generation only. -Domain is only emitted from the explicit FQDN field, never from
// the credential's (NetBIOS) domain.
public static class EnumerationGenerator
{
    public static string Generate(
        string operation,
        string targetDomain,
        bool verbose,
        string credDomain,
        string credUsername)
    {
        bool useCredential = credDomain.Length > 0 && credUsername.Length > 0;
        List<string> lines = new List<string>();

        if (useCredential)
        {
            lines.AddRange(GeneratorHelpers.SecureCredentialBlock(credDomain, credUsername));
        }

        lines.Add(GeneratorHelpers.PowerViewImport);
        lines.Add("");

        string command = operation;
        if (targetDomain.Length > 0)
        {
            command += $" -Domain '{GeneratorHelpers.EscapeSingleQuotes(targetDomain)}'";
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
