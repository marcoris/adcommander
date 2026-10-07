using System.Windows.Forms;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Credentials" tab: domain, username and a masked password field.
// Other tabs reuse these values to build the PowerShell credential object.
public sealed class CredentialTab : UserControl
{
    private readonly TextBox _domainBox = new TextBox();
    private readonly TextBox _usernameBox = new TextBox();
    private readonly TextBox _passwordBox = new TextBox();

    public CredentialTab()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;

        TableLayoutPanel layout = new TableLayoutPanel();
        layout.Dock = DockStyle.Top;
        layout.ColumnCount = 2;
        layout.RowCount = 3;
        layout.AutoSize = true;
        layout.Padding = new Padding(10);
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

        _passwordBox.UseSystemPasswordChar = true; // real password field (input is masked)

        AddRow(layout, 0, "Domain:", _domainBox);
        AddRow(layout, 1, "Username:", _usernameBox);
        AddRow(layout, 2, "Password:", _passwordBox);

        // Short per-field descriptions (spec section 18).
        ToolTip tips = new ToolTip();
        tips.SetToolTip(_domainBox, "The AD/NetBIOS domain, e.g. INLANEFREIGHT.");
        tips.SetToolTip(_usernameBox, "The user the operation runs as, e.g. wley.");
        tips.SetToolTip(_passwordBox, "Only used in the explicit inline mode (insecure). Secure mode asks for it in PowerShell.");

        Controls.Add(layout);
    }

    // Adds one "Label : TextBox" row to the layout.
    private void AddRow(TableLayoutPanel layout, int row, string labelText, TextBox box)
    {
        Label caption = new Label();
        caption.Text = labelText;
        caption.AutoSize = true;
        caption.Anchor = AnchorStyles.Left;
        caption.Margin = new Padding(3, 7, 3, 3);

        box.Width = 300;
        box.Anchor = AnchorStyles.Left;

        layout.Controls.Add(caption, 0, row);
        layout.Controls.Add(box, 1, row);
    }

    // Snapshot of what the user typed. The password is read on demand, not stored long term.
    public CredentialInput GetInput()
    {
        return new CredentialInput
        {
            Domain = _domainBox.Text.Trim(),
            Username = _usernameBox.Text.Trim(),
            Password = _passwordBox.Text
        };
    }
}
