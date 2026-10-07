using System.Windows.Forms;

namespace AD_Commander;

// The application's main window.
// Phase 2: just the window + a TabControl holding the seven (still empty) tabs.
public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();   // keeps the designer-generated setup intact
        BuildUi();               // our own code-built layout
    }

    // Builds the main window layout in code (no designer drag-and-drop),
    // so the whole window is reproducible from this single file.
    private void BuildUi()
    {
        // --- the window itself ---
        Text = "AD Commander";
        Width = 900;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;

        // --- the tab strip that fills the window ---
        TabControl tabs = new TabControl();
        tabs.Dock = DockStyle.Fill;   // grow and shrink together with the window

        // The seven sections from the spec. Each is an empty page for now;
        // the later phases fill them with real controls.
        string[] tabTitles =
        {
            "Credentials",
            "PowerView",
            "ACL",
            "Kerberoasting",
            "AS-REP Roasting",
            "Remoting",
            "Enumeration"
        };

        foreach (string title in tabTitles)
        {
            TabPage page = new TabPage(title);
            tabs.TabPages.Add(page);
        }

        // Put the tab strip onto the window.
        Controls.Add(tabs);
    }
}
