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
            lines.Add("$SecPassword = Read-Host \"Credential password\" -AsSecureString");
            lines.Add($"$Cred = New-Object System.Management.Automation.PSCredential('{Ps(credDomain)}\\{Ps(credUsername)}', $SecPassword)");
            lines.Add("");
        }

        lines.Add("Import-Module .\\PowerView.ps1");
        lines.Add("");

        string command = operation;
        if (targetDomain.Length > 0)
        {
            command += $" -Domain '{Ps(targetDomain)}'";
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

    // Escape single quotes so a value can't break out of a PowerShell single-quoted string.
    private static string Ps(string value) => value.Replace("'", "''");
}
