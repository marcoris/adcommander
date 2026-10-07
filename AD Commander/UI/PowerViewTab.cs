using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "PowerView" tab: pick a Get-Domain* operation and its common parameters.
public sealed class PowerViewTab : UserControl, ICommandTab
{
    private readonly ComboBox _operationBox = new ComboBox();
    private readonly TextBox _identityBox = new TextBox();
    private readonly TextBox _propertiesBox = new TextBox();
    private readonly CheckBox _verboseBox = new CheckBox();

    public PowerViewTab()
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

        _operationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _operationBox.Items.AddRange(new object[]
        {
            "Get-DomainUser",
            "Get-DomainGroup",
            "Get-DomainComputer",
            "Get-DomainObject",
            "Get-DomainGroupMember"
        });
        _operationBox.SelectedIndex = 0;

        _propertiesBox.Text = "*";
        _verboseBox.Text = "Verbose";
        _verboseBox.Checked = true;
        _verboseBox.AutoSize = true;

        AddRow(layout, 0, "Operation:", _operationBox);
        AddRow(layout, 1, "Identity:", _identityBox);
        AddRow(layout, 2, "Properties:", _propertiesBox);

        _verboseBox.Anchor = AnchorStyles.Left;
        _verboseBox.Margin = new Padding(3, 7, 3, 3);
        layout.Controls.Add(_verboseBox, 1, 3);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_operationBox, "The PowerView enumeration cmdlet to run.");
        tips.SetToolTip(_identityBox, "Optional: a specific object to query (user/group/computer name). Empty = all.");
        tips.SetToolTip(_propertiesBox, "Which properties to return. '*' means all; or a comma list like samaccountname,memberof.");

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
        string operation = _operationBox.SelectedItem?.ToString() ?? "Get-DomainUser";

        string script = PowerViewGenerator.Generate(
            operation,
            _identityBox.Text.Trim(),
            _propertiesBox.Text.Trim(),
            _verboseBox.Checked,
            credential.Domain,
            credential.Username);

        return CommandResult.Ok(script);
    }
}
