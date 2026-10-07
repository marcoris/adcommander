using AD_Commander.Models;

namespace AD_Commander.UI;

// Implemented by every tab that can turn its inputs into a PowerShell command.
// The shared Generate button in Form1 calls this on whichever tab is active.
public interface ICommandTab
{
    CommandResult Generate(CredentialInput credential);
}
