using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds a cleanup / rollback script (text only) that undoes temporary AD changes made during a test.
//
// The step order is FIXED here in code — SPN -> group membership -> password — and must NOT follow
// the checkbox/UI order: removing the fake SPN can depend on the group membership still being in
// place, so the SPN is removed first, then the group is left, then the password is restored.
// This order is covered by unit tests.
public static class CleanupGenerator
{
    public static string Generate(
        bool removeSpn,
        bool removeFromGroup,
        bool resetPassword,
        string targetUser,
        string group,
        string spn,
        string originalPassword,
        string credDomain,
        string credUsername,
        string credPassword,
        bool secureMode,
        bool download,
        string downloadUrl)
    {
        List<string> lines = new List<string>();
        lines.Add("# Cleanup: remove temporary AD changes");
        lines.Add("");

        bool useCredential = credDomain.Length > 0 && credUsername.Length > 0;
        if (useCredential)
        {
            lines.AddRange(GeneratorHelpers.CredentialBlock(credDomain, credUsername, credPassword, secureMode));
        }

        lines.Add(GeneratorHelpers.PowerViewImportLine(download, downloadUrl));
        lines.Add("");

        string credSuffix = useCredential ? " -Credential $Cred" : "";
        string user = GeneratorHelpers.EscapeSingleQuotes(targetUser);
        int step = 0;

        // 1. Remove the temporary SPN (first: the permission for this can depend on the group).
        if (removeSpn)
        {
            step++;
            lines.Add($"# {step}. Remove temporary SPN");
            if (spn.Length > 0)
            {
                // Remove exactly this SPN (XOR toggles the value off when present).
                lines.Add($"Set-DomainObject -Identity '{user}' -XOR @{{serviceprincipalname='{GeneratorHelpers.EscapeSingleQuotes(spn)}'}}{credSuffix}");
            }
            else
            {
                // No specific SPN given: clear the whole serviceprincipalname attribute.
                lines.Add($"Set-DomainObject -Identity '{user}' -Clear serviceprincipalname{credSuffix}");
            }
            lines.Add("");
        }

        // 2. Remove the user from the temporary group membership.
        if (removeFromGroup)
        {
            step++;
            lines.Add($"# {step}. Remove user from temporary group membership");
            lines.Add($"Remove-DomainGroupMember -Identity '{GeneratorHelpers.EscapeSingleQuotes(group)}' -Members '{user}'{credSuffix}");
            lines.Add("");
        }

        // 3. Restore / reset the password.
        if (resetPassword)
        {
            step++;
            lines.Add($"# {step}. Restore/reset password");
            if (originalPassword.Length > 0)
            {
                // Case 1: the original password is known and was entered.
                lines.Add($"Set-DomainUserPassword -Identity '{user}' -AccountPassword (ConvertTo-SecureString '{GeneratorHelpers.EscapeSingleQuotes(originalPassword)}' -AsPlainText -Force){credSuffix}");
            }
            else
            {
                // Case 2: the original password is unknown. Never invent one — emit a manual note.
                lines.Add("# Original password is unknown.");
                lines.Add("# The client/user must reset the password manually.");
            }
            lines.Add("");
        }

        return string.Join("\r\n", lines);
    }
}
