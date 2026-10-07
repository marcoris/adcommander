using System.Collections.Generic;

namespace AD_Commander.Generators;

// Builds standard PowerShell remoting commands (Enter-PSSession / Invoke-Command / New-PSSession).
// Pure text generation only. No PowerView import needed.
public static class RemotingGenerator
{
    public static string Generate(
        string operation,
        string computerName,
        string authentication,
        string command,
        string domain,
        string username)
    {
        bool useCredential = domain.Length > 0 && username.Length > 0;
        List<string> lines = new List<string>();

        if (useCredential)
        {
            lines.Add("$SecPassword = Read-Host \"Credential password\" -AsSecureString");
            lines.Add($"$Cred = New-Object System.Management.Automation.PSCredential('{Ps(domain)}\\{Ps(username)}', $SecPassword)");
            lines.Add("");
        }

        // New-PSSession is usually captured in a variable for later Invoke-Command/Enter-PSSession.
        string line = operation == "New-PSSession"
            ? $"$Session = New-PSSession -ComputerName '{Ps(computerName)}'"
            : $"{operation} -ComputerName '{Ps(computerName)}'";

        if (useCredential)
        {
            line += " -Credential $Cred";
        }
        if (authentication.Length > 0 && authentication != "Default")
        {
            line += $" -Authentication {authentication}";
        }
        if (operation == "Invoke-Command")
        {
            // {{ and }} are escaped braces: the output is  -ScriptBlock { <command> }
            line += $" -ScriptBlock {{ {command} }}";
        }

        lines.Add(line);

        return string.Join("\r\n", lines);
    }

    // Escape single quotes so a value can't break out of a PowerShell single-quoted string.
    private static string Ps(string value) => value.Replace("'", "''");
}
