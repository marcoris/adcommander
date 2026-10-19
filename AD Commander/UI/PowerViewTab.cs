using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "PowerView" tab: pick a Group, then a Function. Only the parameters that are valid for the
// selected function are shown (spec section 3). One generator covers all functions.
public sealed class PowerViewTab : UserControl, ICommandTab
{
    // Group -> its functions (order preserved).
    private static readonly (string Group, string[] Functions)[] Groups =
    {
        ("Domain",    new[] { "Get-Domain", "Get-DomainController", "Get-DomainTrust", "Get-Forest", "Get-ForestDomain" }),
        ("Users",     new[] { "Get-DomainUser", "Get-DomainObject" }),
        ("Groups",    new[] { "Get-DomainGroup", "Get-DomainGroupMember" }),
        ("Computers", new[] { "Get-DomainComputer" }),
        ("GPO",       new[] { "Get-DomainGPO", "Get-DomainGPOLocalGroup" }),
        ("Shares",    new[] { "Find-DomainShare" }),
        ("Access",    new[] { "Find-LocalAdminAccess", "Find-DomainUserLocation" }),
    };

    // Function -> which field keys are shown for it (from the real PowerView param blocks).
    // "Verbose" is a common parameter and is always visible, so it is not listed here.
    private static readonly Dictionary<string, string[]> FunctionFields = new()
    {
        ["Get-Domain"]              = new[] { "Domain" },
        ["Get-DomainController"]    = new[] { "Domain", "Server" },
        ["Get-DomainTrust"]        = new[] { "Properties", "Domain", "LDAPFilter", "Server" },
        ["Get-Forest"]             = new[] { "Forest" },
        ["Get-ForestDomain"]       = new[] { "Forest" },
        ["Get-DomainUser"]         = new[] { "Identity", "Properties", "Domain", "LDAPFilter", "Server" },
        ["Get-DomainObject"]       = new[] { "Identity", "Properties", "Domain", "LDAPFilter", "Server" },
        ["Get-DomainGroup"]        = new[] { "Identity", "Properties", "Domain", "LDAPFilter", "Server" },
        ["Get-DomainGroupMember"]  = new[] { "Identity", "Domain", "LDAPFilter", "Server", "Recurse" },
        ["Get-DomainComputer"]     = new[] { "Identity", "Properties", "Domain", "LDAPFilter", "Server" },
        ["Get-DomainGPO"]          = new[] { "Identity", "Properties", "Domain", "LDAPFilter", "Server" },
        ["Get-DomainGPOLocalGroup"]= new[] { "Identity", "Domain", "LDAPFilter", "Server" },
        ["Find-DomainShare"]       = new[] { "ComputerName", "Server" },
        ["Find-LocalAdminAccess"]  = new[] { "ComputerName", "Server" },
        ["Find-DomainUserLocation"]= new[] { "Identity", "Domain", "ComputerName", "Server" },
    };

    private readonly ComboBox _groupBox = new ComboBox();
    private readonly ComboBox _functionBox = new ComboBox();

    private readonly TextBox _identityBox = new TextBox();
    private readonly TextBox _propertiesBox = new TextBox();
    private readonly TextBox _domainBox = new TextBox();
    private readonly TextBox _ldapFilterBox = new TextBox();
    private readonly TextBox _serverBox = new TextBox();
    private readonly TextBox _computerNameBox = new TextBox();
    private readonly TextBox _forestBox = new TextBox();
    private readonly CheckBox _recurseBox = new CheckBox();
    private readonly CheckBox _verboseBox = new CheckBox();

    // field key -> its row container, so we can show/hide whole rows.
    private readonly Dictionary<string, Control> _rows = new();

    public PowerViewTab()
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

        // Group + Function are always visible.
        _groupBox.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (var g in Groups)
        {
            _groupBox.Items.Add(g.Group);
        }
        _groupBox.SelectedIndexChanged += OnGroupChanged;

        _functionBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _functionBox.SelectedIndexChanged += OnFunctionChanged;

        root.Controls.Add(MakeRow("Group:", _groupBox));
        root.Controls.Add(MakeRow("Function:", _functionBox));

        // Dynamic fields (shown/hidden per function).
        _propertiesBox.Text = "*";
        root.Controls.Add(TrackedRow("Identity", "Identity:", _identityBox));
        root.Controls.Add(TrackedRow("Properties", "Properties:", _propertiesBox));
        root.Controls.Add(TrackedRow("Domain", "Domain (FQDN):", _domainBox));
        root.Controls.Add(TrackedRow("LDAPFilter", "LDAP Filter:", _ldapFilterBox));
        root.Controls.Add(TrackedRow("Server", "Server (DC):", _serverBox));
        root.Controls.Add(TrackedRow("ComputerName", "ComputerName:", _computerNameBox));
        root.Controls.Add(TrackedRow("Forest", "Forest:", _forestBox));

        _recurseBox.Text = "Recurse";
        _recurseBox.AutoSize = true;
        root.Controls.Add(TrackedRow("Recurse", "", _recurseBox));

        // Verbose is always available (common parameter).
        _verboseBox.Text = "Verbose";
        _verboseBox.Checked = true;
        _verboseBox.AutoSize = true;
        root.Controls.Add(MakeRow("", _verboseBox));

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_groupBox, "Category of PowerView functions.");
        tips.SetToolTip(_functionBox, "The PowerView cmdlet to run.");
        tips.SetToolTip(_identityBox, "Optional: a specific object (sam/name/DN/SID). Empty = all.");
        tips.SetToolTip(_propertiesBox, "Attributes to return. '*' = all; or a comma list like samaccountname,memberof.");
        tips.SetToolTip(_domainBox, "Target domain FQDN, e.g. inlanefreight.local. Empty = current domain.");
        tips.SetToolTip(_ldapFilterBox, "Raw LDAP filter, e.g. (admincount=1).");
        tips.SetToolTip(_serverBox, "Specific domain controller / LDAP server to query.");
        tips.SetToolTip(_computerNameBox, "Target host(s) to check. Empty = all computers in the domain.");
        tips.SetToolTip(_forestBox, "Target forest name. Empty = current forest.");
        tips.SetToolTip(_recurseBox, "Recurse nested group memberships.");

        Controls.Add(root);

        // Kick off the initial fill + visibility.
        _groupBox.SelectedIndex = 0;
    }

    // A row whose visibility is tracked by key (for show/hide).
    private Control TrackedRow(string key, string caption, Control field)
    {
        TableLayoutPanel row = MakeRow(caption, field);
        _rows[key] = row;
        return row;
    }

    // A 2-column "label : field" row.
    private TableLayoutPanel MakeRow(string caption, Control field)
    {
        TableLayoutPanel row = new TableLayoutPanel();
        row.ColumnCount = 2;
        row.AutoSize = true;
        row.Margin = new Padding(0);
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330f));

        Label cap = new Label();
        cap.Text = caption;
        cap.AutoSize = true;
        cap.Anchor = AnchorStyles.Left;
        cap.Margin = new Padding(3, 7, 3, 3);

        if (field is TextBox)
        {
            field.Width = 310;
        }
        field.Anchor = AnchorStyles.Left;
        field.Margin = new Padding(3, 4, 3, 3);

        row.Controls.Add(cap, 0, 0);
        row.Controls.Add(field, 1, 0);
        return row;
    }

    private void OnGroupChanged(object? sender, EventArgs e)
    {
        string group = _groupBox.SelectedItem?.ToString() ?? "";

        _functionBox.Items.Clear();
        foreach (var g in Groups)
        {
            if (g.Group == group)
            {
                foreach (string fn in g.Functions)
                {
                    _functionBox.Items.Add(fn);
                }
                break;
            }
        }

        if (_functionBox.Items.Count > 0)
        {
            _functionBox.SelectedIndex = 0;   // triggers OnFunctionChanged
        }
    }

    private void OnFunctionChanged(object? sender, EventArgs e)
    {
        string fn = _functionBox.SelectedItem?.ToString() ?? "";
        string[] show = FunctionFields.TryGetValue(fn, out var f) ? f : Array.Empty<string>();

        foreach (KeyValuePair<string, Control> kv in _rows)
        {
            kv.Value.Visible = Array.IndexOf(show, kv.Key) >= 0;
        }
    }

    public CommandResult Generate(CredentialInput credential)
    {
        string fn = _functionBox.SelectedItem?.ToString() ?? "";
        if (fn.Length == 0)
        {
            return CommandResult.Fail("Please select a PowerView function.");
        }

        // Read a text field only if its row is currently visible for this function.
        string Val(string key, TextBox box)
            => (_rows.TryGetValue(key, out var r) && r.Visible) ? box.Text.Trim() : "";

        bool recurse = _rows.TryGetValue("Recurse", out var rr) && rr.Visible && _recurseBox.Checked;

        string script = PowerViewGenerator.Generate(
            fn,
            Val("Identity", _identityBox),
            Val("Properties", _propertiesBox),
            Val("Domain", _domainBox),
            Val("LDAPFilter", _ldapFilterBox),
            Val("Server", _serverBox),
            Val("ComputerName", _computerNameBox),
            Val("Forest", _forestBox),
            recurse,
            _verboseBox.Checked,
            credential.Domain,
            credential.Username);

        return CommandResult.Ok(script);
    }
}
