using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds PowerView enumeration one-liners (Get-Domain* / Find-Domain*). Pure text only.
// One generator covers every grouped function; the tab decides which parameters are relevant.
// The credential block is optional: PowerView often runs in an already-authenticated context.
public static class PowerViewGenerator
{
    public static string Generate(
        string function,
        string identity,
        string properties,
        string domain,
        string ldapFilter,
        string server,
        string computerName,
        string forest,
        bool recurse,
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

        // Only the parameters the user actually filled in (and that are valid for the function) are added.
        string command = function;

        if (identity.Length > 0)
        {
            // Find-DomainUserLocation targets the user via -UserIdentity; every other cmdlet uses -Identity.
            string param = function == "Find-DomainUserLocation" ? "UserIdentity" : "Identity";
            command += $" -{param} '{GeneratorHelpers.EscapeSingleQuotes(identity)}'";
        }
        if (properties.Length > 0)
        {
            command += $" -Properties {properties}";
        }
        if (forest.Length > 0)
        {
            command += $" -Forest '{GeneratorHelpers.EscapeSingleQuotes(forest)}'";
        }
        if (domain.Length > 0)
        {
            command += $" -Domain '{GeneratorHelpers.EscapeSingleQuotes(domain)}'";
        }
        if (ldapFilter.Length > 0)
        {
            command += $" -LDAPFilter '{GeneratorHelpers.EscapeSingleQuotes(ldapFilter)}'";
        }
        if (computerName.Length > 0)
        {
            command += $" -ComputerName '{GeneratorHelpers.EscapeSingleQuotes(computerName)}'";
        }
        if (server.Length > 0)
        {
            command += $" -Server '{GeneratorHelpers.EscapeSingleQuotes(server)}'";
        }
        if (recurse)
        {
            command += " -Recurse";
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
