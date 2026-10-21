using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Certify" tab (Certify 2.0 / GhostPack): pick a verb, only the relevant options are shown.
// Self-contained: Certify takes its own --username/--password, so it does not use the Credentials tab.
// Text generation only; nothing is executed.
public sealed class CertifyTab : UserControl, ICommandTab
{
    private static readonly string[] Verbs =
    {
        "enum-templates", "enum-cas",
        "request", "request-agent", "request-download",
        "forge"
    };

    private static readonly Dictionary<string, string[]> VerbFields = new()
    {
        ["enum-templates"]   = new[] { "Username", "Password", "Domain", "CA", "Template", "FilterVulnerable", "FilterEnabled" },
        ["enum-cas"]         = new[] { "Username", "Password", "Domain", "CA", "FilterVulnerable" },
        ["request"]          = new[] { "CA", "Username", "Password", "Template", "Upn", "Dns", "Sid", "Machine" },
        ["request-agent"]    = new[] { "CA", "Username", "Password", "Template", "Target", "AgentPfx", "AgentPass" },
        ["request-download"] = new[] { "CA", "Username", "Password", "Id" },
        ["forge"]            = new[] { "CaCert", "CaPass", "Subject", "Upn", "Dns", "Sid" },
    };

    private static readonly Dictionary<string, string[]> RequiredFields = new()
    {
        ["request"]          = new[] { "CA", "Template" },
        ["request-agent"]    = new[] { "CA", "Template", "Target", "AgentPfx" },
        ["request-download"] = new[] { "CA", "Id" },
        ["forge"]            = new[] { "CaCert" },
    };

    private readonly ComboBox _verbBox = new ComboBox();

    private readonly TextBox _usernameBox = new TextBox();
    private readonly TextBox _passwordBox = new TextBox();
    private readonly TextBox _domainBox = new TextBox();
    private readonly TextBox _caBox = new TextBox();
    private readonly TextBox _templateBox = new TextBox();
    private readonly TextBox _upnBox = new TextBox();
    private readonly TextBox _dnsBox = new TextBox();
    private readonly TextBox _sidBox = new TextBox();
    private readonly TextBox _targetBox = new TextBox();
    private readonly TextBox _agentPfxBox = new TextBox();
    private readonly TextBox _agentPassBox = new TextBox();
    private readonly TextBox _idBox = new TextBox();
    private readonly TextBox _caCertBox = new TextBox();
    private readonly TextBox _caPassBox = new TextBox();
    private readonly TextBox _subjectBox = new TextBox();
    private readonly CheckBox _filterVulnBox = new CheckBox();
    private readonly CheckBox _filterEnabledBox = new CheckBox();
    private readonly CheckBox _machineBox = new CheckBox();

    private readonly Dictionary<string, Control> _rows = new();

    public CertifyTab()
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

        _verbBox.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (string v in Verbs)
        {
            _verbBox.Items.Add(v);
        }
        _verbBox.SelectedIndexChanged += OnVerbChanged;
        root.Controls.Add(MakeRow("Command:", _verbBox));

        _passwordBox.UseSystemPasswordChar = true;
        _caPassBox.UseSystemPasswordChar = true;
        _agentPassBox.UseSystemPasswordChar = true;

        root.Controls.Add(TrackedRow("Username", "Username:", _usernameBox));
        root.Controls.Add(TrackedRow("Password", "Password:", _passwordBox));
        root.Controls.Add(TrackedRow("Domain", "Domain:", _domainBox));
        root.Controls.Add(TrackedRow("CA", "CA (server\\ca-name):", _caBox));
        root.Controls.Add(TrackedRow("Template", "Template:", _templateBox));
        root.Controls.Add(TrackedRow("Upn", "UPN (SAN):", _upnBox));
        root.Controls.Add(TrackedRow("Dns", "DNS (SAN):", _dnsBox));
        root.Controls.Add(TrackedRow("Sid", "SID:", _sidBox));
        root.Controls.Add(TrackedRow("Target", "Target user:", _targetBox));
        root.Controls.Add(TrackedRow("AgentPfx", "Agent PFX:", _agentPfxBox));
        root.Controls.Add(TrackedRow("AgentPass", "Agent PFX pass:", _agentPassBox));
        root.Controls.Add(TrackedRow("Id", "Request ID:", _idBox));
        root.Controls.Add(TrackedRow("CaCert", "CA cert (PFX):", _caCertBox));
        root.Controls.Add(TrackedRow("CaPass", "CA cert pass:", _caPassBox));
        root.Controls.Add(TrackedRow("Subject", "Subject:", _subjectBox));

        _filterVulnBox.Text = "--filter-vulnerable";
        _filterVulnBox.AutoSize = true;
        root.Controls.Add(TrackedRow("FilterVulnerable", "", _filterVulnBox));

        _filterEnabledBox.Text = "--filter-enabled";
        _filterEnabledBox.AutoSize = true;
        root.Controls.Add(TrackedRow("FilterEnabled", "", _filterEnabledBox));

        _machineBox.Text = "--machine (machine template)";
        _machineBox.AutoSize = true;
        root.Controls.Add(TrackedRow("Machine", "", _machineBox));

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_verbBox, "The Certify command to build.");
        tips.SetToolTip(_usernameBox, "LDAP username for enumeration/request (optional; defaults to current context).");
        tips.SetToolTip(_passwordBox, "Password for the LDAP username.");
        tips.SetToolTip(_domainBox, "Target domain FQDN.");
        tips.SetToolTip(_caBox, "Certificate authority as SERVER\\CA-NAME.");
        tips.SetToolTip(_templateBox, "Certificate template name.");
        tips.SetToolTip(_upnBox, "Subject Alternative Name UPN, e.g. administrator@domain.local (ESC1).");
        tips.SetToolTip(_dnsBox, "Subject Alternative Name DNS.");
        tips.SetToolTip(_sidBox, "Target SID for the SAN (ESC1/strong mapping).");
        tips.SetToolTip(_targetBox, "The user to request on behalf of (ESC3 enrollment agent).");
        tips.SetToolTip(_agentPfxBox, "Path to the enrollment-agent PFX.");
        tips.SetToolTip(_agentPassBox, "Password for the enrollment-agent PFX.");
        tips.SetToolTip(_idBox, "The CA request ID to download.");
        tips.SetToolTip(_caCertBox, "CA private key (PFX/P12) to forge with.");
        tips.SetToolTip(_caPassBox, "Password for the CA private key.");
        tips.SetToolTip(_subjectBox, "Subject name, e.g. CN=User.");

        Controls.Add(root);

        _verbBox.SelectedIndex = 0;   // triggers OnVerbChanged
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
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
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

    private void OnVerbChanged(object? sender, EventArgs e)
    {
        string verb = _verbBox.SelectedItem?.ToString() ?? "";
        string[] show = VerbFields.TryGetValue(verb, out var f) ? f : Array.Empty<string>();

        foreach (KeyValuePair<string, Control> kv in _rows)
        {
            kv.Value.Visible = Array.IndexOf(show, kv.Key) >= 0;
        }
    }

    public CommandResult Generate(CredentialInput credential)
    {
        // credential (shared Credentials tab) is intentionally unused: Certify takes its own --username/--password.
        string verb = _verbBox.SelectedItem?.ToString() ?? "";
        if (verb.Length == 0)
        {
            return CommandResult.Fail("Please select a Certify command.");
        }

        string Val(string key, TextBox box)
            => (_rows.TryGetValue(key, out var r) && r.Visible) ? box.Text.Trim() : "";

        string username = Val("Username", _usernameBox);
        string password = Val("Password", _passwordBox);
        string domain = Val("Domain", _domainBox);
        string ca = Val("CA", _caBox);
        string template = Val("Template", _templateBox);
        string upn = Val("Upn", _upnBox);
        string dns = Val("Dns", _dnsBox);
        string sid = Val("Sid", _sidBox);
        string target = Val("Target", _targetBox);
        string agentPfx = Val("AgentPfx", _agentPfxBox);
        string agentPass = Val("AgentPass", _agentPassBox);
        string id = Val("Id", _idBox);
        string caCert = Val("CaCert", _caCertBox);
        string caPass = Val("CaPass", _caPassBox);
        string subject = Val("Subject", _subjectBox);

        bool filterVuln = _rows.TryGetValue("FilterVulnerable", out var fv) && fv.Visible && _filterVulnBox.Checked;
        bool filterEnabled = _rows.TryGetValue("FilterEnabled", out var fe) && fe.Visible && _filterEnabledBox.Checked;
        bool machine = _rows.TryGetValue("Machine", out var mc) && mc.Visible && _machineBox.Checked;

        if (RequiredFields.TryGetValue(verb, out var required))
        {
            string ValueFor(string key) => key switch
            {
                "CA" => ca,
                "Template" => template,
                "Target" => target,
                "AgentPfx" => agentPfx,
                "Id" => id,
                "CaCert" => caCert,
                _ => ""
            };

            List<(string, string)> pairs = new List<(string, string)>();
            foreach (string key in required)
            {
                pairs.Add((key, ValueFor(key)));
            }

            string? error = Validation.MissingRequired(pairs.ToArray());
            if (error != null)
            {
                return CommandResult.Fail(verb + ": " + error);
            }
        }

        string script = CertifyGenerator.Generate(
            verb, username, password, domain, ca, template, upn, dns, sid,
            target, agentPfx, agentPass, id, caCert, caPass, subject,
            filterVuln, filterEnabled, machine);

        return CommandResult.Ok(script);
    }
}
