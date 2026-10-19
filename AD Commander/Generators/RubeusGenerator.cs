using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds Rubeus.exe command lines. Pure text generation only.
// The tab shows only the parameters valid for the selected command, so every value passed in here
// is already scoped to that command; this generator just appends whatever is filled in.
public static class RubeusGenerator
{
    public static string Generate(
        string command,
        string user,
        string secretType,
        string secretValue,
        string password,
        string domain,
        string dc,
        string service,
        string spn,
        string ticket,
        string impersonateUser,
        string msdsSpn,
        string altService,
        string luid,
        string server,
        string target,
        string format,
        string ou,
        string outfile,
        bool ptt,
        bool nowrap)
    {
        List<string> lines = new List<string>();

        lines.Add("# Rubeus: https://github.com/GhostPack/Rubeus");
        lines.Add("# Precompiled binaries: https://github.com/r3motecontrol/Ghostpack-CompiledBinaries");
        lines.Add("");

        string cmd = "Rubeus.exe " + command;

        if (user.Length > 0)
        {
            cmd += $" /user:{user}";
        }
        if (secretValue.Length > 0)
        {
            // Password / hash is passed under the switch matching its type.
            string sw = secretType switch
            {
                "RC4" => "rc4",
                "AES128" => "aes128",
                "AES256" => "aes256",
                _ => "password"
            };
            cmd += $" /{sw}:{secretValue}";
        }
        if (password.Length > 0)
        {
            cmd += $" /password:{password}";
        }
        if (domain.Length > 0)
        {
            cmd += $" /domain:{domain}";
        }
        if (dc.Length > 0)
        {
            cmd += $" /dc:{dc}";
        }
        if (service.Length > 0)
        {
            cmd += $" /service:{Quote(service)}";
        }
        if (spn.Length > 0)
        {
            cmd += $" /spn:{Quote(spn)}";
        }
        if (ticket.Length > 0)
        {
            cmd += $" /ticket:{Quote(ticket)}";
        }
        if (impersonateUser.Length > 0)
        {
            cmd += $" /impersonateuser:{impersonateUser}";
        }
        if (msdsSpn.Length > 0)
        {
            cmd += $" /msdsspn:{Quote(msdsSpn)}";
        }
        if (altService.Length > 0)
        {
            cmd += $" /altservice:{Quote(altService)}";
        }
        if (luid.Length > 0)
        {
            cmd += $" /luid:{luid}";
        }
        if (server.Length > 0)
        {
            cmd += $" /server:{server}";
        }
        if (target.Length > 0)
        {
            cmd += $" /target:{Quote(target)}";
        }
        if (format.Length > 0)
        {
            cmd += $" /format:{format.ToLowerInvariant()}";
        }
        if (ou.Length > 0)
        {
            cmd += $" /ou:{Quote(ou)}";
        }
        if (outfile.Length > 0)
        {
            cmd += $" /outfile:{Quote(outfile)}";
        }
        if (ptt)
        {
            cmd += " /ptt";
        }
        if (nowrap)
        {
            cmd += " /nowrap";
        }

        lines.Add(cmd);

        return string.Join("\r\n", lines);
    }

    // Wrap a value in double quotes (for SPNs, paths, DNs and tickets that may contain spaces/slashes).
    private static string Quote(string value) => "\"" + value + "\"";
}
