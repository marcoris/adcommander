using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using AD_Commander.Models;
using AD_Commander.UI;

namespace AD_Commander;

public partial class Form1 : Form
{
    private readonly RichTextBox _outputBox = new RichTextBox();
    private readonly CredentialTab _credentialTab = new CredentialTab();
    private readonly TreeView _navigator = new TreeView();
    private readonly Panel _contentPanel = new Panel();
    private readonly GuidancePanel _guidance = new GuidancePanel();

    // Registered operation tabs (title + control). Credentials is NOT here — it is fixed top-left.
    private readonly List<(string Title, UserControl Control)> _tabs = new();
    private UserControl? _activeControl;

    public Form1()
    {
        InitializeComponent();
        BuildUi();
    }

    private void BuildUi()
    {
        Text = "AD Commander";
        Width = 960;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(820, 520);

        // Register the operation tabs (grouped/sorted in the navigator by their Strang/Phase).
        RegisterTab("PowerView", new PowerViewTab());
        RegisterTab("ACL", new AclTab());
        RegisterTab("Kerberoasting", new KerberoastTab());
        RegisterTab("AS-REP Roasting", new AsRepRoastTab());
        RegisterTab("Rubeus", new RubeusTab());
        RegisterTab("SharpHound", new SharpHoundTab());
        RegisterTab("Certify", new CertifyTab());
        RegisterTab("Remoting", new RemotingTab());
        RegisterTab("Cleanup", new CleanupTab());

        SplitContainer split = new SplitContainer();
        split.Dock = DockStyle.Fill;
        split.FixedPanel = FixedPanel.Panel1;

        // --- Left: Credentials button (fixed) + navigator tree + guidance panel ---
        TableLayoutPanel left = new TableLayoutPanel();
        left.Dock = DockStyle.Fill;
        left.ColumnCount = 1;
        left.RowCount = 3;
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34f));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 140f));

        Button credButton = new Button();
        credButton.Text = "Credentials";
        credButton.Dock = DockStyle.Fill;
        credButton.Click += OnCredentialsClicked;

        _navigator.Dock = DockStyle.Fill;
        _navigator.HideSelection = false;
        _navigator.AfterSelect += OnNavigatorSelect;

        _guidance.StateChanged += OnOperatorStateChanged;

        left.Controls.Add(credButton, 0, 0);
        left.Controls.Add(_navigator, 0, 1);
        left.Controls.Add(_guidance, 0, 2);
        split.Panel1.Controls.Add(left);

        // --- Right: content area + output + buttons ---
        TableLayoutPanel right = new TableLayoutPanel();
        right.Dock = DockStyle.Fill;
        right.ColumnCount = 1;
        right.RowCount = 4;
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 24f));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 190f));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));

        _contentPanel.Dock = DockStyle.Fill;

        _outputBox.Dock = DockStyle.Fill;
        _outputBox.Font = new Font("Consolas", 10f);
        _outputBox.WordWrap = false;
        _outputBox.ScrollBars = RichTextBoxScrollBars.Both;

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

        right.Controls.Add(_contentPanel, 0, 0);
        right.Controls.Add(outputLabel, 0, 1);
        right.Controls.Add(_outputBox, 0, 2);
        right.Controls.Add(buttons, 0, 3);
        split.Panel2.Controls.Add(right);

        Controls.Add(split);
        split.SplitterDistance = 220;

        BuildNavigator();
        UpdateNavigatorGating();
        ShowControl(_credentialTab);   // start on the Credentials screen
    }

    private void RegisterTab(string title, UserControl control)
    {
        _tabs.Add((title, control));
    }

    // Build the navigator: one node per Strang, its tabs sorted by Phase.
    private void BuildNavigator()
    {
        _navigator.BeginUpdate();
        _navigator.Nodes.Clear();

        AttackStrang[] order = { AttackStrang.NoCredentials, AttackStrang.Credentialed, AttackStrang.Cleanup };
        foreach (AttackStrang strang in order)
        {
            TreeNode strangNode = new TreeNode(StrangLabel(strang));
            strangNode.Tag = strang;

            IEnumerable<(string Title, UserControl Control)> inStrang = _tabs
                .Where(t => t.Control is ICommandTab ct && ct.Strang == strang)
                .OrderBy(t => (int)((ICommandTab)t.Control).Phase);

            foreach ((string Title, UserControl Control) t in inStrang)
            {
                strangNode.Nodes.Add(new TreeNode(t.Title) { Tag = t.Control });
            }

            if (strangNode.Nodes.Count > 0)
            {
                _navigator.Nodes.Add(strangNode);
            }
        }

        _navigator.ExpandAll();
        _navigator.EndUpdate();
    }

    private static string StrangLabel(AttackStrang strang) => strang switch
    {
        AttackStrang.NoCredentials => "No credentials",
        AttackStrang.Credentialed => "Credentialed",
        AttackStrang.Cleanup => "Cleanup",
        _ => strang.ToString()
    };

    private void OnCredentialsClicked(object? sender, EventArgs e)
    {
        _navigator.SelectedNode = null;
        ShowControl(_credentialTab);
    }

    private void OnNavigatorSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node?.Tag is UserControl control)
        {
            ShowControl(control);
        }
    }

    // Show one control in the content area; keep every control's state by hiding, not disposing.
    private void ShowControl(UserControl control)
    {
        if (!_contentPanel.Controls.Contains(control))
        {
            control.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(control);
        }

        foreach (Control c in _contentPanel.Controls)
        {
            c.Visible = (c == control);
        }

        control.BringToFront();
        _activeControl = control;
    }

    private void OnOperatorStateChanged(object? sender, EventArgs e)
    {
        UpdateNavigatorGating();
    }

    // Grey out strands (and their tabs) that the current operator state can't reach yet. Guidance only.
    private void UpdateNavigatorGating()
    {
        OperatorState state = _guidance.CurrentState;
        foreach (TreeNode strangNode in _navigator.Nodes)
        {
            if (strangNode.Tag is AttackStrang strang)
            {
                bool available = AttackFlow.IsAvailable(strang, state);
                string baseLabel = StrangLabel(strang);
                strangNode.Text = available
                    ? baseLabel
                    : baseLabel + "  (locked: " + AttackFlow.LockReason(strang) + ")";

                Color color = available ? SystemColors.WindowText : SystemColors.GrayText;
                strangNode.ForeColor = color;
                foreach (TreeNode leaf in strangNode.Nodes)
                {
                    leaf.ForeColor = color;
                }
            }
        }
    }

    // Ask the active tab to generate, passing in the shared credentials.
    private void OnGenerateClicked(object? sender, EventArgs e)
    {
        if (_activeControl is ICommandTab commandTab)
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
            "Select an operation in the navigator on the left, fill in its fields, then click Generate.",
            "AD Commander", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

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

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _outputBox.Clear();
    }
}
