using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "ACL" tab. First operation: PowerView Set-DomainUserPassword (ForceChangePassword abuse).
// Domain/Username/credential password come from the shared Credentials tab; this tab adds the
// target and the new password. Secure vs inline is the global switch on the Credentials tab.
public sealed class AclTab : UserControl, ICommandTab
{
    private readonly TextBox _targetIdentityBox = new TextBox();
    private readonly TextBox _newPasswordBox = new TextBox();

    public AclTab()
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
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320f));

        Label operation = new Label();
        operation.Text = "Operation: Set-DomainUserPassword";
        operation.AutoSize = true;
        operation.Margin = new Padding(3, 3, 3, 10);
        layout.Controls.Add(operation, 0, 0);
        layout.SetColumnSpan(operation, 2);

        _newPasswordBox.UseSystemPasswordChar = true;

        AddRow(layout, 1, "Target Identity:", _targetIdentityBox);
        AddRow(layout, 2, "New Password:", _newPasswordBox);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_targetIdentityBox, "The AD user whose password is changed, e.g. damundsen.");
        tips.SetToolTip(_newPasswordBox, "The new password for the target. Only embedded in inline mode.");

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

    // Validate the inputs, then hand off to the generator.
    public CommandResult Generate(CredentialInput credential)
    {
        string targetIdentity = _targetIdentityBox.Text.Trim();
        bool inline = !credential.SecureMode;   // global switch on the Credentials tab

        if (credential.Domain.Length == 0 || credential.Username.Length == 0)
        {
            return CommandResult.Fail("Please fill in Domain and Username on the Credentials tab.");
        }

        if (targetIdentity.Length == 0)
        {
            return CommandResult.Fail("Please enter the Target Identity.");
        }

        if (inline && (credential.Password.Length == 0 || _newPasswordBox.Text.Length == 0))
        {
            return CommandResult.Fail("Inline mode (Credentials tab) needs the Credentials-tab password and a New Password.");
        }

        string script = SetDomainUserPasswordGenerator.Generate(
            credential.Domain,
            credential.Username,
            targetIdentity,
            credential.Password,
            _newPasswordBox.Text,
            secureMode: !inline);

        return CommandResult.Ok(script);
    }
}
