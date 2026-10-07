using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Kerberoasting" tab: PowerView Invoke-Kerberoast with output format and optional target.
public sealed class KerberoastTab : UserControl, ICommandTab
{
    private readonly TextBox _targetUserBox = new TextBox();
    private readonly ComboBox _outputFormatBox = new ComboBox();
    private readonly CheckBox _verboseBox = new CheckBox();

    public KerberoastTab()
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

        _outputFormatBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _outputFormatBox.Items.AddRange(new object[] { "Hashcat", "John" });
        _outputFormatBox.SelectedIndex = 0;

        _verboseBox.Text = "Verbose";
        _verboseBox.Checked = true;
        _verboseBox.AutoSize = true;

        AddRow(layout, 0, "Target user:", _targetUserBox);
        AddRow(layout, 1, "Output format:", _outputFormatBox);

        _verboseBox.Anchor = AnchorStyles.Left;
        _verboseBox.Margin = new Padding(3, 7, 3, 3);
        layout.Controls.Add(_verboseBox, 1, 2);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_targetUserBox, "Optional: restrict to one SPN account. Empty = roast all SPN accounts.");
        tips.SetToolTip(_outputFormatBox, "Hash format for cracking: Hashcat or John.");

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
        string outputFormat = _outputFormatBox.SelectedItem?.ToString() ?? "Hashcat";

        string script = KerberoastGenerator.Generate(
            _targetUserBox.Text.Trim(),
            outputFormat,
            _verboseBox.Checked,
            credential.Domain,
            credential.Username);

        return CommandResult.Ok(script);
    }
}
