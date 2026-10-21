using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds Certify 2.0 (GhostPack) command lines for AD CS enumeration/abuse. Text only.
// The tab shows only the options valid for the selected verb, so every value passed here is
// already scoped to that verb; this generator just appends whatever is filled in.
public static class CertifyGenerator
{
    public static string Generate(
        string verb,
        string username,
        string password,
        string domain,
        string ca,
        string template,
        string upn,
        string dns,
        string sid,
        string target,
        string agentPfx,
        string agentPass,
        string id,
        string caCert,
        string caPass,
        string subject,
        bool filterVulnerable,
        bool filterEnabled,
        bool machine)
    {
        List<string> lines = new List<string>();
        lines.Add("# Certify 2.0 (GhostPack): https://github.com/GhostPack/Certify");
        lines.Add("");

        List<string> parts = new List<string> { "Certify.exe", verb };

        Opt(parts, "username", username);
        Opt(parts, "password", password);
        Opt(parts, "domain", domain);
        Opt(parts, "ca", ca);
        Opt(parts, "template", template);
        Opt(parts, "upn", upn);
        Opt(parts, "dns", dns);
        Opt(parts, "sid", sid);
        Opt(parts, "target", target);
        Opt(parts, "agent-pfx", agentPfx);
        Opt(parts, "agent-pass", agentPass);
        Opt(parts, "id", id);
        Opt(parts, "ca-cert", caCert);
        Opt(parts, "ca-pass", caPass);
        Opt(parts, "subject", subject);

        if (filterVulnerable)
        {
            parts.Add("--filter-vulnerable");
        }
        if (filterEnabled)
        {
            parts.Add("--filter-enabled");
        }
        if (machine)
        {
            parts.Add("--machine");
        }

        lines.Add(string.Join(" ", parts));

        return string.Join("\r\n", lines);
    }

    // Append "--name value" (value quoted) when the value is non-empty.
    private static void Opt(List<string> parts, string name, string value)
    {
        if (value.Length > 0)
        {
            parts.Add("--" + name + " " + Quote(value));
        }
    }

    private static string Quote(string value) => "\"" + value + "\"";
}
