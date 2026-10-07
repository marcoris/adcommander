using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "AS-REP Roasting" tab: PowerView enumeration or Rubeus asreproast.
public sealed class AsRepRoastTab : UserControl, ICommandTab
{
    private readonly ComboBox _toolBox = new ComboBox();
    private readonly TextBox _identityBox = new TextBox();
    private readonly ComboBox _outputFormatBox = new ComboBox();
    private readonly TextBox _propertiesBox = new TextBox();
    private readonly CheckBox _verboseBox = new CheckBox();

    public AsRepRoastTab()
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

        _toolBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _toolBox.Items.AddRange(new object[] { "PowerView", "Rubeus" });
        _toolBox.SelectedIndex = 0;

        _outputFormatBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _outputFormatBox.Items.AddRange(new object[] { "Hashcat", "John" });
        _outputFormatBox.SelectedIndex = 0;

        _propertiesBox.Text = "samaccountname,userprincipalname";
        _verboseBox.Text = "Verbose";
        _verboseBox.Checked = true;
        _verboseBox.AutoSize = true;

        AddRow(layout, 0, "Tool:", _toolBox);
        AddRow(layout, 1, "Identity:", _identityBox);
        AddRow(layout, 2, "Output format:", _outputFormatBox);
        AddRow(layout, 3, "Properties:", _propertiesBox);

        _verboseBox.Anchor = AnchorStyles.Left;
        _verboseBox.Margin = new Padding(3, 7, 3, 3);
        layout.Controls.Add(_verboseBox, 1, 4);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_toolBox, "PowerView (enumerate only) or Rubeus (find and extract hashes).");
        tips.SetToolTip(_identityBox, "Optional: restrict to one user. Empty = all AS-REP roastable accounts.");
        tips.SetToolTip(_outputFormatBox, "Hash format for cracking (Rubeus).");
        tips.SetToolTip(_propertiesBox, "Which properties to return (PowerView).");
        tips.SetToolTip(_verboseBox, "PowerView only.");

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
        string tool = _toolBox.SelectedItem?.ToString() ?? "PowerView";
        string outputFormat = _outputFormatBox.SelectedItem?.ToString() ?? "Hashcat";

        string script = AsRepRoastGenerator.Generate(
            tool,
            _identityBox.Text.Trim(),
            _propertiesBox.Text.Trim(),
            outputFormat,
            _verboseBox.Checked,
            credential.Domain,
            credential.Username,
            credential.Password);

        return CommandResult.Ok(script);
    }
}
