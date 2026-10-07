using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Enumeration" tab: broad PowerView domain/trust/GPO/share discovery.
public sealed class EnumerationTab : UserControl, ICommandTab
{
    private readonly ComboBox _operationBox = new ComboBox();
    private readonly TextBox _domainBox = new TextBox();
    private readonly CheckBox _verboseBox = new CheckBox();

    public EnumerationTab()
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

        _operationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _operationBox.Items.AddRange(new object[]
        {
            "Get-Domain",
            "Get-DomainController",
            "Get-DomainTrust",
            "Get-DomainGPO",
            "Find-LocalAdminAccess",
            "Find-DomainShare"
        });
        _operationBox.SelectedIndex = 0;

        _verboseBox.Text = "Verbose";
        _verboseBox.Checked = true;
        _verboseBox.AutoSize = true;

        AddRow(layout, 0, "Operation:", _operationBox);
        AddRow(layout, 1, "Domain (FQDN):", _domainBox);

        _verboseBox.Anchor = AnchorStyles.Left;
        _verboseBox.Margin = new Padding(3, 7, 3, 3);
        layout.Controls.Add(_verboseBox, 1, 2);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_operationBox, "A broad PowerView discovery command for the whole domain.");
        tips.SetToolTip(_domainBox, "Optional DNS domain to target, e.g. inlanefreight.local. Empty = current domain.");

        Controls.Add(layout);
    }

    private void AddRow(TableLayoutPanel layout, int row, string labelText, Control field)
    {
        Label caption = new Label();
        caption.Text = labelText;
        caption.AutoSize = true;
        caption.Anchor = AnchorStyles.Left;
        caption.Margin = new Padding(3, 7, 3, 3);

        field.Width = 300;
        field.Anchor = AnchorStyles.Left;

        layout.Controls.Add(caption, 0, row);
        layout.Controls.Add(field, 1, row);
    }

    public CommandResult Generate(CredentialInput credential)
    {
        string operation = _operationBox.SelectedItem?.ToString() ?? "Get-Domain";

        string script = EnumerationGenerator.Generate(
            operation,
            _domainBox.Text.Trim(),
            _verboseBox.Checked,
            credential.Domain,
            credential.Username);

        return CommandResult.Ok(script);
    }
}
