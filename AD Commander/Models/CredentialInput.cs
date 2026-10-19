namespace AD_Commander.Models;

// Plain data holder for the credential fields entered on the Credentials tab.
//
// SecureMode is the global switch between the two credential styles:
//   true  (default) -> PowerShell prompts for the password at runtime (Read-Host); Password stays unused.
//   false (inline)  -> the Password below is embedded in the generated script (lab use only).
public sealed class CredentialInput
{
    public string Domain { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool SecureMode { get; set; } = true;
}
