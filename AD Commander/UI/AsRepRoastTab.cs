using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "AS-REP Roasting" tab: PowerView enumeration of accounts without Kerberos pre-auth.
public sealed class AsRepRoastTab : UserControl, ICommandTab
{
    private readonly TextBox _identityBox = new TextBox();
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

        _propertiesBox.Text = "samaccountname,userprincipalname";
        _verboseBox.Text = "Verbose";
        _verboseBox.Checked = true;
        _verboseBox.AutoSize = true;

        AddRow(layout, 0, "Identity:", _identityBox);
        AddRow(layout, 1, "Properties:", _propertiesBox);

        _verboseBox.Anchor = AnchorStyles.Left;
        _verboseBox.Margin = new Padding(3, 7, 3, 3);
        layout.Controls.Add(_verboseBox, 1, 2);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_identityBox, "Optional: restrict to one user. Empty = all AS-REP roastable accounts.");
        tips.SetToolTip(_propertiesBox, "Which properties to return for the found accounts.");

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
        string script = AsRepRoastGenerator.Generate(
            _identityBox.Text.Trim(),
            _propertiesBox.Text.Trim(),
            _verboseBox.Checked,
            credential.Domain,
            credential.Username);

        return CommandResult.Ok(script);
    }
}
