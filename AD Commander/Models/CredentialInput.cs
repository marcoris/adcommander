namespace AD_Commander.Models;

// Plain data holder for the credential fields entered on the Credentials tab.
//
// Security note: in the default "secure" mode (added in a later phase) the password is
// entered in PowerShell via `Read-Host -AsSecureString` and is NOT taken from here, so the
namespace AD_Commander.Models;

// Plain data holder for the credential fields entered on the Credentials tab.
//
// Security note: in the default "secure" mode (added in a later phase) the password is
// entered in PowerShell via `Read-Host -AsSecureString` and is NOT taken from here, so the
// Password string stays empty. It is only used in the explicitly marked, insecure inline mode.
public sealed class CredentialInput
{
    public string Domain { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
