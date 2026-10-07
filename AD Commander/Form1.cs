using System;
using System.Drawing;
using System.Windows.Forms;

namespace AD_Commander;

// The application's main window.
// Phase 3: the seven tabs on top, a monospace output area, and the button row.
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

        // Root layout: three stacked rows (tabs, output, buttons). A TableLayoutPanel is
        // used instead of plain docking so the regions always keep their place.
        TableLayoutPanel root = new TableLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.ColumnCount = 1;
        root.RowCount = 3;
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // tabs take the remaining space
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 190f)); // output: fixed height
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));  // buttons: fixed height

        // --- Row 0: the seven tabs (same as phase 2) ---
        TabControl tabs = new TabControl();
        tabs.Dock = DockStyle.Fill;
        string[] tabTitles =
        {
            "Credentials", "PowerView", "ACL", "Kerberoasting",
            "AS-REP Roasting", "Remoting", "Enumeration"
        };

        foreach (string title in tabTitles)
        {
            tabs.TabPages.Add(new TabPage(title));
        }

        // --- Row 1: the output area where generated PowerShell is shown ---
        _outputBox.Dock = DockStyle.Fill;
        _outputBox.Font = new Font("Consolas", 10f);
        _outputBox.WordWrap = false;          // keep PowerShell lines intact
        _outputBox.ScrollBars = RichTextBoxScrollBars.Both;
    }
}
