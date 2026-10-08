using System;
using System.Drawing;
using System.Windows.Forms;
using AD_Commander.Models;
using AD_Commander.UI;

namespace AD_Commander;

// The application's main window.
// Phase 5/6: ACL tab + Set-DomainUserPassword generator wired to the shared Generate button.
public partial class Form1 : Form
{
    private readonly RichTextBox _outputBox = new RichTextBox();
    private readonly TabControl _tabs = new TabControl();
    private readonly CredentialTab _credentialTab = new CredentialTab();

    public Form1()
    {
        InitializeComponent();
        BuildUi();
    }

    private void BuildUi()
    {
        Text = "AD Commander";
        Width = 900;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(760, 520);

        TableLayoutPanel root = new TableLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.ColumnCount = 1;
        root.RowCount = 4;
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));

        // --- Row 0: the tabs ---
        _tabs.Dock = DockStyle.Fill;

        TabPage credentialsPage = new TabPage("Credentials");
        credentialsPage.Controls.Add(_credentialTab);
        _tabs.TabPages.Add(credentialsPage);

        TabPage powerViewPage = new TabPage("PowerView");
        powerViewPage.Controls.Add(new PowerViewTab());
        _tabs.TabPages.Add(powerViewPage);

        TabPage aclPage = new TabPage("ACL");
        aclPage.Controls.Add(new AclTab());
        _tabs.TabPages.Add(aclPage);

        TabPage kerberoastPage = new TabPage("Kerberoasting");
        kerberoastPage.Controls.Add(new KerberoastTab());
        _tabs.TabPages.Add(kerberoastPage);
        
        TabPage asRepPage = new TabPage("AS-REP Roasting");
        asRepPage.Controls.Add(new AsRepRoastTab());
        _tabs.TabPages.Add(asRepPage);
        
        TabPage remotingPage = new TabPage("Remoting");
        remotingPage.Controls.Add(new RemotingTab());
        _tabs.TabPages.Add(remotingPage);
        
        TabPage enumerationPage = new TabPage("Enumeration");
        enumerationPage.Controls.Add(new EnumerationTab());
        _tabs.TabPages.Add(enumerationPage);

        // --- Row 1: the output area ---
        _outputBox.Dock = DockStyle.Fill;
        _outputBox.Font = new Font("Consolas", 10f);
        _outputBox.WordWrap = false;
        _outputBox.ScrollBars = RichTextBoxScrollBars.Both;

        // --- Row 2: the three buttons ---
        FlowLayoutPanel buttons = new FlowLayoutPanel();
        buttons.Dock = DockStyle.Fill;
        buttons.FlowDirection = FlowDirection.LeftToRight;
        buttons.Padding = new Padding(4);

        Button generateButton = new Button();
        generateButton.Text = "Generate";
        generateButton.AutoSize = true;
        generateButton.Click += OnGenerateClicked;

        Button copyButton = new Button();
        copyButton.Text = "Copy to Clipboard";
        copyButton.AutoSize = true;
        copyButton.Click += OnCopyClicked;

        Button clearButton = new Button();
        clearButton.Text = "Clear";
        clearButton.AutoSize = true;
        clearButton.Click += OnClearClicked;

        buttons.Controls.Add(generateButton);
        buttons.Controls.Add(copyButton);
        buttons.Controls.Add(clearButton);

        Label outputLabel = new Label();
        outputLabel.Text = "Generated PowerShell:";
        outputLabel.AutoSize = true;
        outputLabel.Margin = new Padding(3, 3, 3, 0);

        root.Controls.Add(_tabs, 0, 0);
        root.Controls.Add(outputLabel, 0, 1);
        root.Controls.Add(_outputBox, 0, 2);
        root.Controls.Add(buttons, 0, 3);

        Controls.Add(root);
    }

    // Ask the active tab to generate, passing in the shared credentials.
    private void OnGenerateClicked(object? sender, EventArgs e)
    {
        TabPage? page = _tabs.SelectedTab;
        Control? content = (page != null && page.Controls.Count > 0) ? page.Controls[0] : null;

        if (content is ICommandTab commandTab)
        {
            CommandResult result = commandTab.Generate(_credentialTab.GetInput());
            if (result.Success)
            {
                _outputBox.Text = result.Text;
            }
            else
            {
                MessageBox.Show(result.Error, "AD Commander",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return;
        }

        MessageBox.Show(
            "Select an operation tab (e.g. ACL), fill in its fields, then click Generate.",
            "AD Commander", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // Copy the current output to the Windows clipboard, if there is any.
    private void OnCopyClicked(object? sender, EventArgs e)
    {
        if (_outputBox.TextLength == 0)
        {
            MessageBox.Show("There is nothing to copy yet.", "AD Commander",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Clipboard.SetText(_outputBox.Text);
    }

    // Empty the output area.
    private void OnClearClicked(object? sender, EventArgs e)
    {
        _outputBox.Clear();
    }
}
