using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Remoting" tab: Enter-PSSession / Invoke-Command / New-PSSession.
public sealed class RemotingTab : UserControl, ICommandTab
{
    private readonly ComboBox _operationBox = new ComboBox();
    private readonly TextBox _computerNameBox = new TextBox();
    private readonly ComboBox _authBox = new ComboBox();
    private readonly TextBox _commandBox = new TextBox();

    public RemotingTab()
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

        _operationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _operationBox.Items.AddRange(new object[] { "Enter-PSSession", "Invoke-Command", "New-PSSession" });
        _operationBox.SelectedIndex = 0;

        _authBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _authBox.Items.AddRange(new object[] { "Default", "Kerberos", "Negotiate", "CredSSP", "NTLM", "Basic", "Digest" });
        _authBox.SelectedIndex = 0;

        AddRow(layout, 0, "Operation:", _operationBox);
        AddRow(layout, 1, "ComputerName:", _computerNameBox);
        AddRow(layout, 2, "Authentication:", _authBox);
        AddRow(layout, 3, "Command:", _commandBox);

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_operationBox, "Which remoting cmdlet to build.");
        tips.SetToolTip(_computerNameBox, "The target host, e.g. DC01 or 10.10.10.5.");
        tips.SetToolTip(_authBox, "Authentication mechanism. Default lets PowerShell choose.");
        tips.SetToolTip(_commandBox, "The PowerShell to run remotely. Used only by Invoke-Command (as -ScriptBlock).");

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
        string operation = _operationBox.SelectedItem?.ToString() ?? "Enter-PSSession";
        string computerName = _computerNameBox.Text.Trim();
        string authentication = _authBox.SelectedItem?.ToString() ?? "Default";
        string command = _commandBox.Text.Trim();

        if (computerName.Length == 0)
        {
            return CommandResult.Fail("Please enter a ComputerName.");
        }

        if (operation == "Invoke-Command" && command.Length == 0)
        {
            return CommandResult.Fail("Invoke-Command needs a Command to run.");
        }

        string script = RemotingGenerator.Generate(
            operation,
            computerName,
            authentication,
            command,
            credential.Domain,
            credential.Username);

        return CommandResult.Ok(script);
    }
}
