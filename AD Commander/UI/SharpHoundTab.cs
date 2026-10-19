using System.Collections.Generic;
using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "SharpHound" tab: build a SharpHound.exe collection command. Text only, no execution.
// Collection methods are multi-select; the rest maps to the common SharpHound flags.
public sealed class SharpHoundTab : UserControl, ICommandTab
{
    // Collection methods from the real SharpHound Options (order as defined there).
    private static readonly string[] Methods =
    {
        "Default", "All", "DCOnly",
        "Group", "LocalGroup", "LocalAdmin", "RDP", "DCOM", "PSRemote",
        "Session", "LoggedOn", "Trusts", "ACL", "Container", "ComputerOnly",
        "GPOLocalGroup", "ObjectProps", "SPNTargets", "UserRights",
        "CARegistry", "DCRegistry", "CertServices", "Site",
        "WebClientService", "LdapServices", "SmbInfo", "NTLMRegistry"
    };

    private readonly CheckedListBox _methodsBox = new CheckedListBox();
    private readonly TextBox _domainBox = new TextBox();
    private readonly TextBox _dcBox = new TextBox();
    private readonly TextBox _outputDirBox = new TextBox();
    private readonly TextBox _outputPrefixBox = new TextBox();
    private readonly CheckBox _loopBox = new CheckBox();
    private readonly CheckBox _stealthBox = new CheckBox();
    private readonly CheckBox _zipBox = new CheckBox();

    public SharpHoundTab()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;

        TableLayoutPanel layout = new TableLayoutPanel();
        layout.Dock = DockStyle.Top;
        layout.ColumnCount = 2;
        layout.AutoSize = true;
        layout.Padding = new Padding(10);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

        _methodsBox.Items.AddRange(Methods);
        _methodsBox.CheckOnClick = true;
        _methodsBox.Height = 150;
        _methodsBox.Width = 300;
        _methodsBox.Anchor = AnchorStyles.Left;
        // Pre-check "Default".
        for (int i = 0; i < _methodsBox.Items.Count; i++)
        {
            if (_methodsBox.Items[i]?.ToString() == "Default")
            {
                _methodsBox.SetItemChecked(i, true);
                break;
            }
        }

        _zipBox.Checked = true;

        AddRow(layout, 0, "Collection Methods:", _methodsBox);
        AddRow(layout, 1, "Domain:", _domainBox);
        AddRow(layout, 2, "Domain Controller:", _dcBox);
        AddRow(layout, 3, "Output Directory:", _outputDirBox);
        AddRow(layout, 4, "Output Prefix:", _outputPrefixBox);

        _loopBox.Text = "Loop (--Loop)";
        _loopBox.AutoSize = true;
        AddCheck(layout, 5, _loopBox);

        _stealthBox.Text = "Stealth (--stealth)";
        _stealthBox.AutoSize = true;
        AddCheck(layout, 6, _stealthBox);

        _zipBox.Text = "Zip output (uncheck for --nozip)";
        _zipBox.AutoSize = true;
        AddCheck(layout, 7, _zipBox);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_methodsBox, "Which collection methods to run. 'Default' is a sensible baseline; combine as needed.");
        tips.SetToolTip(_domainBox, "Domain to enumerate, e.g. inlanefreight.local. Empty = current domain.");
        tips.SetToolTip(_dcBox, "Override which DC to pull LDAP from. Can cause data loss if misused.");
        tips.SetToolTip(_outputDirBox, "Directory to write the output to. Empty = current directory.");
        tips.SetToolTip(_outputPrefixBox, "String prepended to output file names.");
        tips.SetToolTip(_loopBox, "Loop computer collection (sessions/local groups over time).");
        tips.SetToolTip(_stealthBox, "Stealth collection (prefers DCOnly where possible).");
        tips.SetToolTip(_zipBox, "SharpHound zips by default; uncheck to emit raw JSON (--nozip).");

        Controls.Add(layout);
    }

    private void AddRow(TableLayoutPanel layout, int row, string labelText, Control field)
    {
        Label caption = new Label();
        caption.Text = labelText;
        caption.AutoSize = true;
        caption.Anchor = AnchorStyles.Left;
        caption.Margin = new Padding(3, 7, 3, 3);

        if (field is TextBox)
        {
            field.Width = 300;
        }
        field.Anchor = AnchorStyles.Left;

        layout.Controls.Add(caption, 0, row);
        layout.Controls.Add(field, 1, row);
    }

    private void AddCheck(TableLayoutPanel layout, int row, CheckBox box)
    {
        box.Anchor = AnchorStyles.Left;
        box.Margin = new Padding(3, 5, 3, 3);
        layout.Controls.Add(box, 1, row);
    }

    public CommandResult Generate(CredentialInput credential)
    {
        // credential (shared Credentials tab) is intentionally unused: SharpHound runs in the
        // current user's context; alternate LDAP creds are out of scope for this first pass.
        List<string> methods = new List<string>();
        foreach (object item in _methodsBox.CheckedItems)
        {
            methods.Add(item.ToString() ?? "");
        }

        string script = SharpHoundGenerator.Generate(
            methods,
            _domainBox.Text.Trim(),
            _dcBox.Text.Trim(),
            _outputDirBox.Text.Trim(),
            _outputPrefixBox.Text.Trim(),
            _loopBox.Checked,
            _stealthBox.Checked,
            _zipBox.Checked);

        return CommandResult.Ok(script);
    }
}
