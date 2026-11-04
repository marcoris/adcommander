namespace AD_Commander.Models;

// Which "strand" of the engagement a tab belongs to. Drives the navigator grouping.
public enum AttackStrang
{
    NoCredentials,
    Credentialed,
    Cleanup
}

// Kill-chain phase of a tab, with explicit numeric order (100-900) used for sorting.
public enum AttackPhase
{
    NoCredEnumeration = 100,
    NoCredAttacks = 200,
    Enumeration = 300,
    Kerberos = 400,
    CredentialAccess = 500,
    AclAbuse = 600,
    ADCS = 700,
    Cleanup = 900
}
