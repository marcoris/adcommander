using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Kerberoasting" tab: PowerView Invoke-Kerberoast or Rubeus kerberoast.
public sealed class KerberoastTab : UserControl, ICommandTab
{
    private readonly ComboBox _toolBox = new ComboBox();
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

        _toolBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _toolBox.Items.AddRange(new object[] { "PowerView", "Rubeus" });
        _toolBox.SelectedIndex = 0;

        _outputFormatBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _outputFormatBox.Items.AddRange(new object[] { "Hashcat", "John" });
        _outputFormatBox.SelectedIndex = 0;

        _verboseBox.Text = "Verbose";
        _verboseBox.Checked = true;
        _verboseBox.AutoSize = true;

        AddRow(layout, 0, "Tool:", _toolBox);
        AddRow(layout, 1, "Target user:", _targetUserBox);
        AddRow(layout, 2, "Output format:", _outputFormatBox);

        _verboseBox.Anchor = AnchorStyles.Left;
        _verboseBox.Margin = new Padding(3, 7, 3, 3);
        layout.Controls.Add(_verboseBox, 1, 3);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_toolBox, "PowerView (Invoke-Kerberoast, secure prompt) or Rubeus (compiled .exe).");
        tips.SetToolTip(_targetUserBox, "Optional: restrict to one SPN account. Empty = roast all SPN accounts.");
        tips.SetToolTip(_outputFormatBox, "Hash format for cracking: Hashcat or John.");
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

        string script = KerberoastGenerator.Generate(
            tool,
            _targetUserBox.Text.Trim(),
            outputFormat,
            _verboseBox.Checked,
            credential.Domain,
            credential.Username,
            credential.Password);

        return CommandResult.Ok(script);
    }
}
