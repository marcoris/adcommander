using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "Cleanup" tab: pick which temporary changes to undo; generates a rollback script as text.
// The actual step order (SPN -> group -> password) is fixed in CleanupGenerator, NOT by this UI.
// Credentials + global PowerView delivery come from the Credentials tab. Nothing is executed.
public sealed class CleanupTab : UserControl, ICommandTab
{
    private readonly CheckBox _removeSpnBox = new CheckBox();
    private readonly CheckBox _removeGroupBox = new CheckBox();
    private readonly CheckBox _resetPwBox = new CheckBox();

    private readonly TextBox _targetUserBox = new TextBox();
    private readonly TextBox _spnBox = new TextBox();
    private readonly TextBox _groupBox = new TextBox();
    private readonly TextBox _originalPwBox = new TextBox();

    // field key -> its row container, so we can show/hide whole rows.
    private readonly Dictionary<string, Control> _rows = new();

    public CleanupTab()
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
        root.AutoScroll = true;
        root.Padding = new Padding(10);

        Label header = new Label();
        header.Text = "Cleanup actions:";
        header.AutoSize = true;
        header.Margin = new Padding(3, 3, 3, 3);
        root.Controls.Add(header);

        // Action checkboxes (always visible). Order here is cosmetic; the script order is fixed.
        _removeSpnBox.Text = "Remove fake SPN";
        _removeSpnBox.AutoSize = true;
        _removeSpnBox.CheckedChanged += OnActionChanged;
        root.Controls.Add(MakeRow("", _removeSpnBox));

        _removeGroupBox.Text = "Remove user from group";
        _removeGroupBox.AutoSize = true;
        _removeGroupBox.CheckedChanged += OnActionChanged;
        root.Controls.Add(MakeRow("", _removeGroupBox));

        _resetPwBox.Text = "Restore/reset password";
        _resetPwBox.AutoSize = true;
        _resetPwBox.CheckedChanged += OnActionChanged;
        root.Controls.Add(MakeRow("", _resetPwBox));

        // Target user is always relevant.
        root.Controls.Add(MakeRow("Target User:", _targetUserBox));

        // Per-action fields (shown only when their action is selected).
        root.Controls.Add(TrackedRow("SPN", "SPN:", _spnBox));
        root.Controls.Add(TrackedRow("Group", "Group:", _groupBox));
        root.Controls.Add(TrackedRow("OriginalPassword", "Original Password:", _originalPwBox));

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_removeSpnBox, "Undo a fake SPN set for targeted Kerberoasting.");
        tips.SetToolTip(_removeGroupBox, "Remove the user from a group it was temporarily added to.");
        tips.SetToolTip(_resetPwBox, "Restore the user's password (if the original is known) or emit a manual note.");
        tips.SetToolTip(_targetUserBox, "The user whose temporary changes are undone, e.g. damundsen.");
        tips.SetToolTip(_spnBox, "The exact SPN to remove (e.g. nonexistent/svc). Empty = clear all SPNs.");
        tips.SetToolTip(_groupBox, "The group to remove the user from.");
        tips.SetToolTip(_originalPwBox, "The original password. If empty, the script notes a manual reset instead of inventing one.");

        Controls.Add(root);

        UpdateFieldVisibility();
    }

    private Control TrackedRow(string key, string caption, Control field)
    {
        TableLayoutPanel row = MakeRow(caption, field);
        _rows[key] = row;
        return row;
    }

    private TableLayoutPanel MakeRow(string caption, Control field)
    {
        TableLayoutPanel row = new TableLayoutPanel();
        row.ColumnCount = 2;
        row.AutoSize = true;
        row.Margin = new Padding(0);
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330f));

        Label cap = new Label();
        cap.Text = caption;
        cap.AutoSize = true;
        cap.Anchor = AnchorStyles.Left;
        cap.Margin = new Padding(3, 7, 3, 3);

        if (field is TextBox)
        {
            field.Width = 310;
        }
        field.Anchor = AnchorStyles.Left;
        field.Margin = new Padding(3, 4, 3, 3);

        row.Controls.Add(cap, 0, 0);
        row.Controls.Add(field, 1, 0);
        return row;
    }

    private void OnActionChanged(object? sender, EventArgs e)
    {
        UpdateFieldVisibility();
    }

    private void UpdateFieldVisibility()
    {
        _rows["SPN"].Visible = _removeSpnBox.Checked;
        _rows["Group"].Visible = _removeGroupBox.Checked;
        _rows["OriginalPassword"].Visible = _resetPwBox.Checked;
    }

    public CommandResult Generate(CredentialInput credential)
    {
        bool removeSpn = _removeSpnBox.Checked;
        bool removeGroup = _removeGroupBox.Checked;
        bool resetPw = _resetPwBox.Checked;

        if (!removeSpn && !removeGroup && !resetPw)
        {
            return CommandResult.Fail("Select at least one cleanup action.");
        }

        string targetUser = _targetUserBox.Text.Trim();

        List<(string, string)> required = new List<(string, string)> { ("Target User", targetUser) };
        if (removeGroup)
        {
            required.Add(("Group", _groupBox.Text.Trim()));
        }

        string? error = Validation.MissingRequired(required.ToArray());
        if (error != null)
        {
            return CommandResult.Fail(error);
        }

        string script = CleanupGenerator.Generate(
            removeSpn,
            removeGroup,
            resetPw,
            targetUser,
            _groupBox.Text.Trim(),
            removeSpn ? _spnBox.Text.Trim() : "",
            resetPw ? _originalPwBox.Text : "",
            credential.Domain,
            credential.Username,
            credential.Password,
            credential.SecureMode,
            credential.DownloadPowerView,
            credential.PowerViewUrl);

        return CommandResult.Ok(script);
    }
}
