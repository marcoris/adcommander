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
    // including the trailing blank line.
    public static List<string> SecureCredentialBlock(string domain, string username)
    {
        return new List<string>
        {
            "$SecPassword = Read-Host \"Credential password\" -AsSecureString",
            $"$Cred = New-Object System.Management.Automation.PSCredential('{EscapeSingleQuotes(domain)}\\{EscapeSingleQuotes(username)}', $SecPassword)",
            ""
        };
    }

    // The "inline" credential block: the password is embedded in the script in clear text.
    // Lab use only. Returns the lines to insert, including the trailing blank line.
    public static List<string> InlineCredentialBlock(string domain, string username, string password)
    {
        return new List<string>
        {
            "# !!! INSECURE: the password is embedded in clear text below. Use only in an isolated lab. !!!",
            $"$SecPassword = ConvertTo-SecureString '{EscapeSingleQuotes(password)}' -AsPlainText -Force",
            $"$Cred = New-Object System.Management.Automation.PSCredential('{EscapeSingleQuotes(domain)}\\{EscapeSingleQuotes(username)}', $SecPassword)",
            ""
        };
    }

    // Pick the credential block based on the global SecureMode flag.
    public static List<string> CredentialBlock(string domain, string username, string password, bool secureMode)
        => secureMode
            ? SecureCredentialBlock(domain, username)
            : InlineCredentialBlock(domain, username, password);

    // The PowerView import line used by every PowerView-based generator.
    public const string PowerViewImport = "Import-Module .\\PowerView.ps1";
}
