using System;

namespace AD_Commander.Models;

// What the operator has reached. Set manually in the GuidancePanel (no auto-tracking).
// Guidance only — it gates what the navigator shows as available, nothing is enforced.
[Flags]
public enum OperatorState
{
    None = 0,
    ValidCredential = 1,
    DomainUser = 2,
    LocalAdmin = 4,
    DomainAdmin = 8
}
