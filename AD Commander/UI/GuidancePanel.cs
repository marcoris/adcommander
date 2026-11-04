using System;
using System.Windows.Forms;
using AD_Commander.Models;

namespace AD_Commander.UI;

// Lets the operator declare what they've reached (manual, not auto-tracked). Raises StateChanged
// so Form1 can re-gate the navigator. Guidance only — nothing is enforced.
public sealed class GuidancePanel : UserControl
{
    private readonly CheckBox _validCredential = new CheckBox();
    private readonly CheckBox _domainUser = new CheckBox();
    private readonly CheckBox _localAdmin = new CheckBox();
    private readonly CheckBox _domainAdmin = new CheckBox();

    public event EventHandler? StateChanged;

    public OperatorState CurrentState
    {
        get
        {
            OperatorState s = OperatorState.None;
            if (_validCredential.Checked) s |= OperatorState.ValidCredential;
            if (_domainUser.Checked) s |= OperatorState.DomainUser;
            if (_localAdmin.Checked) s |= OperatorState.LocalAdmin;
            if (_domainAdmin.Checked) s |= OperatorState.DomainAdmin;
            return s;
        }
    }

    public GuidancePanel()
    {
        BuildUi();
    }

    private void BuildUi()
    {
        Dock = DockStyle.Fill;

        FlowLayoutPanel root = new FlowLayoutPanel();
        root.Dock = DockStyle.Fill;
        root.FlowDirection = FlowDirection.TopDown;
        root.WrapContents = false;
        root.Padding = new Padding(6, 4, 4, 4);

        Label header = new Label();
        header.Text = "Operator state (what you've reached):";
        header.AutoSize = true;
        header.Margin = new Padding(3, 3, 3, 3);
        root.Controls.Add(header);

        AddCheck(root, _validCredential, "Valid credential");
        AddCheck(root, _domainUser, "Domain user");
        AddCheck(root, _localAdmin, "Local admin");
        AddCheck(root, _domainAdmin, "Domain admin");

        Controls.Add(root);
    }

    private void AddCheck(Control parent, CheckBox box, string text)
    {
        box.Text = text;
        box.AutoSize = true;
        box.Margin = new Padding(3, 2, 3, 2);
        box.CheckedChanged += (s, e) => StateChanged?.Invoke(this, EventArgs.Empty);
        parent.Controls.Add(box);
    }
}
