namespace AD_Commander.Models;

// Gating rules between the operator state and the attack strands. Guidance only.
public static class AttackFlow
{
    private const OperatorState AnyCredential =
        OperatorState.ValidCredential | OperatorState.DomainUser | OperatorState.LocalAdmin | OperatorState.DomainAdmin;

    // No-credential and cleanup strands are always available; the credentialed strand needs some credential.
    public static bool IsAvailable(AttackStrang strang, OperatorState state)
        => strang switch
        {
            AttackStrang.Credentialed => (state & AnyCredential) != 0,
            _ => true
        };

    public static string LockReason(AttackStrang strang)
        => strang switch
        {
            AttackStrang.Credentialed => "requires a valid credential / domain user",
            _ => ""
        };
}
