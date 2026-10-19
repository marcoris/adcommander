using System.Collections.Generic;

namespace AD_Commander.Generators;

// Small shared helpers for the command generators. Intentionally minimal: only the
// pieces that were duplicated across several generators live here (spec section 2).
public static class GeneratorHelpers
{
    // Escape single quotes so a value can't break out of a PowerShell single-quoted string.
    public static string EscapeSingleQuotes(string value) => value.Replace("'", "''");

    // The default "secure" credential block: PowerShell prompts for the password at runtime
    // (Read-Host -AsSecureString), so it never enters this app. Returns the lines to insert,
    // including the trailing blank line, matching the existing generators' output exactly.
    public static List<string> SecureCredentialBlock(string domain, string username)
    {
        return new List<string>
        {
            "$SecPassword = Read-Host \"Credential password\" -AsSecureString",
            $"$Cred = New-Object System.Management.Automation.PSCredential('{EscapeSingleQuotes(domain)}\\{EscapeSingleQuotes(username)}', $SecPassword)",
            ""
        };
    }

    // The PowerView import line used by every PowerView-based generator.
    public const string PowerViewImport = "Import-Module .\\PowerView.ps1";
}
