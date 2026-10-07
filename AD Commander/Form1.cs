using System;
using System.Drawing;
using System.Windows.Forms;
using AD_Commander.UI;

namespace AD_Commander;

// The application's main window.
// Phase 4: the Credentials tab now hosts the CredentialTab control; other tabs still empty.
public partial class Form1 : Form
{
    // Kept as a field so later phases (the Generate button) can write generated code here.
    private readonly RichTextBox _outputBox = new RichTextBox();

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

        // Root layout: three stacked rows (tabs, output, buttons).
        TableLayoutPanel root = new TableLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.ColumnCount = 1;
        root.RowCount = 3;
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));

        // --- Row 0: the tabs ---
        TabControl tabs = new TabControl();
        tabs.Dock = DockStyle.Fill;

        // The Credentials tab hosts its own control; the rest stay empty for now.
        TabPage credentialsPage = new TabPage("Credentials");
        credentialsPage.Controls.Add(new CredentialTab());
        tabs.TabPages.Add(credentialsPage);

        string[] remainingTabs =
        {
            "PowerView", "ACL", "Kerberoasting", "AS-REP Roasting", "Remoting", "Enumeration"
        };
        foreach (string title in remainingTabs)
        {
            tabs.TabPages.Add(new TabPage(title));
        }

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

        root.Controls.Add(tabs, 0, 0);
        root.Controls.Add(_outputBox, 0, 1);
        root.Controls.Add(buttons, 0, 2);

        Controls.Add(root);
    }

    // Generate is wired to real command generators from phase 6 on.
    private void OnGenerateClicked(object? sender, EventArgs e)
    {
        _outputBox.Text =
            "# Generate will produce PowerShell here once the first\r\n" +
            "# command generator is added (phase 6).";
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
