using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Rubeus" tab: pick a command, then only the parameters valid for it are shown.
// Self-contained: Rubeus' credential model is per-command, so this tab does NOT use the
// shared Credentials tab. Text generation only; nothing is executed.
public sealed class RubeusTab : UserControl, ICommandTab
{
    // Commands offered in the first pass (spec section 4).
    private static readonly string[] Commands =
    {
        "asktgt", "asktgs", "s4u", "renew",
        "ptt", "purge", "describe", "klist", "triage", "dump",
        "tgtdeleg", "hash",
        "kerberoast", "asreproast"
    };

    // Command -> which field keys are shown for it (from the real Rubeus usage).
    private static readonly Dictionary<string, string[]> CommandFields = new()
    {
        ["asktgt"]     = new[] { "User", "SecretType", "SecretValue", "Domain", "DC", "PTT", "NoWrap" },
        ["asktgs"]     = new[] { "Ticket", "Service", "DC", "PTT", "NoWrap" },
        ["s4u"]        = new[] { "User", "SecretType", "SecretValue", "ImpersonateUser", "MSDSSPN", "AltService", "Domain", "DC", "PTT", "NoWrap" },
        ["renew"]      = new[] { "Ticket", "DC", "PTT", "NoWrap" },
        ["ptt"]        = new[] { "Ticket", "LUID" },
        ["purge"]      = new[] { "LUID" },
        ["describe"]   = new[] { "Ticket" },
        ["klist"]      = new[] { "LUID", "User", "Service", "Server" },
        ["triage"]     = new[] { "LUID", "User", "Service", "Server" },
        ["dump"]       = new[] { "LUID", "User", "Service", "Server", "NoWrap" },
        ["tgtdeleg"]   = new[] { "Target" },
        ["hash"]       = new[] { "Password", "User", "Domain" },
        ["kerberoast"] = new[] { "User", "SPN", "Domain", "DC", "Format", "OU", "Outfile", "NoWrap" },
        ["asreproast"] = new[] { "User", "Domain", "DC", "Format", "OU", "Outfile", "NoWrap" },
    };

    // Command -> fields that must be filled in for a usable command (hard requirements only).
    private static readonly Dictionary<string, string[]> RequiredFields = new()
    {
        ["asktgt"]   = new[] { "User", "SecretValue" },
        ["asktgs"]   = new[] { "Ticket", "Service" },
        ["s4u"]      = new[] { "User", "SecretValue", "ImpersonateUser" },
        ["renew"]    = new[] { "Ticket" },
        ["ptt"]      = new[] { "Ticket" },
        ["describe"] = new[] { "Ticket" },
        ["hash"]     = new[] { "Password" },
    };

    private readonly ComboBox _commandBox = new ComboBox();

    private readonly TextBox _userBox = new TextBox();
    private readonly ComboBox _secretTypeBox = new ComboBox();
    private readonly TextBox _secretValueBox = new TextBox();
    private readonly TextBox _passwordBox = new TextBox();
    private readonly TextBox _domainBox = new TextBox();
    private readonly TextBox _dcBox = new TextBox();
    private readonly TextBox _serviceBox = new TextBox();
    private readonly TextBox _spnBox = new TextBox();
    private readonly TextBox _ticketBox = new TextBox();
    private readonly TextBox _impersonateBox = new TextBox();
    private readonly TextBox _msdsSpnBox = new TextBox();
    private readonly TextBox _altServiceBox = new TextBox();
    private readonly TextBox _luidBox = new TextBox();
    private readonly TextBox _serverBox = new TextBox();
    private readonly TextBox _targetBox = new TextBox();
    private readonly ComboBox _formatBox = new ComboBox();
    private readonly TextBox _ouBox = new TextBox();
    private readonly TextBox _outfileBox = new TextBox();
    private readonly CheckBox _pttBox = new CheckBox();
    private readonly CheckBox _nowrapBox = new CheckBox();

    // field key -> its row container, so we can show/hide whole rows.
    private readonly Dictionary<string, Control> _rows = new();

    public RubeusTab()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;

        FlowLayoutPanel root = new FlowLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.FlowDirection = FlowDirection.TopDown;
        root.WrapContents = false;
        root.AutoScroll = true;
        root.Padding = new Padding(10);

        _commandBox.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (string c in Commands)
        {
            _commandBox.Items.Add(c);
        }
        _commandBox.SelectedIndexChanged += OnCommandChanged;
        root.Controls.Add(MakeRow("Command:", _commandBox));

        _secretTypeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _secretTypeBox.Items.AddRange(new object[] { "Password", "RC4", "AES128", "AES256" });
        _secretTypeBox.SelectedIndex = 0;

        _formatBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _formatBox.Items.AddRange(new object[] { "Hashcat", "John" });
        _formatBox.SelectedIndex = 0;

        _passwordBox.UseSystemPasswordChar = true;

        // Dynamic fields (shown/hidden per command).
        root.Controls.Add(TrackedRow("User", "User:", _userBox));
        root.Controls.Add(TrackedRow("SecretType", "Secret type:", _secretTypeBox));
        root.Controls.Add(TrackedRow("SecretValue", "Secret (pw/hash):", _secretValueBox));
        root.Controls.Add(TrackedRow("Password", "Password:", _passwordBox));
        root.Controls.Add(TrackedRow("Domain", "Domain:", _domainBox));
        root.Controls.Add(TrackedRow("DC", "DC:", _dcBox));
        root.Controls.Add(TrackedRow("Service", "Service (SPN):", _serviceBox));
        root.Controls.Add(TrackedRow("SPN", "SPN filter:", _spnBox));
        root.Controls.Add(TrackedRow("Ticket", "Ticket (b64/file):", _ticketBox));
        root.Controls.Add(TrackedRow("ImpersonateUser", "Impersonate:", _impersonateBox));
        root.Controls.Add(TrackedRow("MSDSSPN", "msds SPN:", _msdsSpnBox));
        root.Controls.Add(TrackedRow("AltService", "Alt service:", _altServiceBox));
        root.Controls.Add(TrackedRow("LUID", "LUID:", _luidBox));
        root.Controls.Add(TrackedRow("Server", "Server:", _serverBox));
        root.Controls.Add(TrackedRow("Target", "Target (SPN):", _targetBox));
        root.Controls.Add(TrackedRow("Format", "Format:", _formatBox));
        root.Controls.Add(TrackedRow("OU", "OU:", _ouBox));
        root.Controls.Add(TrackedRow("Outfile", "Outfile:", _outfileBox));

        _pttBox.Text = "/ptt (pass the ticket)";
        _pttBox.AutoSize = true;
        root.Controls.Add(TrackedRow("PTT", "", _pttBox));

        _nowrapBox.Text = "/nowrap";
        _nowrapBox.AutoSize = true;
        root.Controls.Add(TrackedRow("NoWrap", "", _nowrapBox));

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_commandBox, "The Rubeus command to build.");
        tips.SetToolTip(_userBox, "Target username (no domain part).");
        tips.SetToolTip(_secretTypeBox, "How the secret is supplied: plaintext password or a key/hash.");
        tips.SetToolTip(_secretValueBox, "The password or the matching hash/key for the chosen type.");
        tips.SetToolTip(_passwordBox, "Plaintext password to hash.");
        tips.SetToolTip(_domainBox, "Target domain FQDN.");
        tips.SetToolTip(_dcBox, "Specific domain controller to talk to.");
        tips.SetToolTip(_serviceBox, "Requested service SPN(s), e.g. cifs/host.domain.com.");
        tips.SetToolTip(_spnBox, "SPN filter for roasting, e.g. MSSQLSvc/sql01.");
        tips.SetToolTip(_ticketBox, "Existing ticket: base64 blob or path to a .kirbi file.");
        tips.SetToolTip(_impersonateBox, "User to impersonate via S4U.");
        tips.SetToolTip(_msdsSpnBox, "The msds-allowedtodelegateto SPN, e.g. cifs/host.");
        tips.SetToolTip(_altServiceBox, "Alternate service to substitute into the final ticket.");
        tips.SetToolTip(_luidBox, "Target logon session LUID (requires elevation).");
        tips.SetToolTip(_serverBox, "Target server to filter on, e.g. DC01.DOMAIN.COM.");
        tips.SetToolTip(_targetBox, "Target SPN for the delegation ticket.");
        tips.SetToolTip(_formatBox, "Hash output format for cracking.");
        tips.SetToolTip(_ouBox, "Restrict to an OU, e.g. OU=Servers,DC=domain,DC=com.");
        tips.SetToolTip(_outfileBox, "Write hashes/tickets to this file.");

        Controls.Add(root);

        _commandBox.SelectedIndex = 0;   // triggers OnCommandChanged
    }

    private Control TrackedRow(string key, string caption, Control field)
    {
        TableLayoutPanel row = MakeRow(caption, field);
        _rows[key] = row;
        return row;
    }

    private TableLayoutPanel MakeRow(string caption, Control field)
    {
        TableLayoutPanel row = new TableLayoutPanel();
        row.ColumnCount = 2;
        row.AutoSize = true;
        row.Margin = new Padding(0);
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330f));

        Label cap = new Label();
        cap.Text = caption;
        cap.AutoSize = true;
        cap.Anchor = AnchorStyles.Left;
        cap.Margin = new Padding(3, 7, 3, 3);

        if (field is TextBox || field is ComboBox)
        {
            field.Width = 310;
        }
        field.Anchor = AnchorStyles.Left;
        field.Margin = new Padding(3, 4, 3, 3);

        row.Controls.Add(cap, 0, 0);
        row.Controls.Add(field, 1, 0);
        return row;
    }

    private void OnCommandChanged(object? sender, EventArgs e)
    {
        string command = _commandBox.SelectedItem?.ToString() ?? "";
        string[] show = CommandFields.TryGetValue(command, out var f) ? f : Array.Empty<string>();

        foreach (KeyValuePair<string, Control> kv in _rows)
        {
            kv.Value.Visible = Array.IndexOf(show, kv.Key) >= 0;
        }
    }

    // Human-readable label for a required field key (used in validation messages).
    private static string LabelFor(string key) => key switch
    {
        "SecretValue" => "Secret",
        "ImpersonateUser" => "Impersonate",
        _ => key
    };

    public CommandResult Generate(CredentialInput credential)
    {
        // credential (shared Credentials tab) is intentionally unused: Rubeus credentials are per-command.
        string command = _commandBox.SelectedItem?.ToString() ?? "";
        if (command.Length == 0)
        {
            return CommandResult.Fail("Please select a Rubeus command.");
        }

        // Read a field only if its row is currently visible for this command.
        string Val(string key, TextBox box)
            => (_rows.TryGetValue(key, out var r) && r.Visible) ? box.Text.Trim() : "";

        string user = Val("User", _userBox);
        string secretValue = Val("SecretValue", _secretValueBox);
        string password = Val("Password", _passwordBox);
        string domain = Val("Domain", _domainBox);
        string dc = Val("DC", _dcBox);
        string service = Val("Service", _serviceBox);
        string spn = Val("SPN", _spnBox);
        string ticket = Val("Ticket", _ticketBox);
        string impersonate = Val("ImpersonateUser", _impersonateBox);
        string msdsSpn = Val("MSDSSPN", _msdsSpnBox);
        string altService = Val("AltService", _altServiceBox);
        string luid = Val("LUID", _luidBox);
        string server = Val("Server", _serverBox);
        string target = Val("Target", _targetBox);
        string ou = Val("OU", _ouBox);
        string outfile = Val("Outfile", _outfileBox);

        string secretType = (_rows.TryGetValue("SecretType", out var st) && st.Visible)
            ? (_secretTypeBox.SelectedItem?.ToString() ?? "Password")
            : "Password";

        string format = (_rows.TryGetValue("Format", out var fo) && fo.Visible)
            ? (_formatBox.SelectedItem?.ToString() ?? "Hashcat")
            : "";

        bool ptt = _rows.TryGetValue("PTT", out var pr) && pr.Visible && _pttBox.Checked;
        bool nowrap = _rows.TryGetValue("NoWrap", out var nr) && nr.Visible && _nowrapBox.Checked;

        // Check the command's hard-required fields, if any.
        if (RequiredFields.TryGetValue(command, out var required))
        {
            string ValueFor(string key) => key switch
            {
                "User" => user,
                "SecretValue" => secretValue,
                "Password" => password,
                "Ticket" => ticket,
                "Service" => service,
                "ImpersonateUser" => impersonate,
                _ => ""
            };

            List<(string, string)> pairs = new List<(string, string)>();
            foreach (string key in required)
            {
                pairs.Add((LabelFor(key), ValueFor(key)));
            }

            string? error = Validation.MissingRequired(pairs.ToArray());
            if (error != null)
            {
                return CommandResult.Fail(command + ": " + error);
            }
        }

        string script = RubeusGenerator.Generate(
            command,
            user,
            secretType,
            secretValue,
            password,
            domain,
            dc,
            service,
            spn,
            ticket,
            impersonate,
            msdsSpn,
            altService,
            luid,
            server,
            target,
            format,
            ou,
            outfile,
            ptt,
            nowrap);

        return CommandResult.Ok(script);
    }
}
