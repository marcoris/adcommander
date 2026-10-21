using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AD_Commander.Generators;
using AD_Commander.Models;

namespace AD_Commander.UI;

// The "ACL" tab: pick an ACL-abuse operation, then only the fields valid for it are shown.
// Structure: Operation / Target / Principal / Parameter (spec section 7).
// Domain/Username/credential password + secure-vs-inline come from the shared Credentials tab.
public sealed class AclTab : UserControl, ICommandTab
{
    private static readonly string[] Operations =
    {
        "Set-DomainUserPassword",
        "Add-DomainGroupMember",
        "Add-DomainObjectAcl",
        "Set-DomainObject"
    };

    // Operation -> which field keys are shown.
    private static readonly Dictionary<string, string[]> OperationFields = new()
    {
        ["Set-DomainUserPassword"] = new[] { "Target", "NewPassword" },
        ["Add-DomainGroupMember"]  = new[] { "Target", "Principal" },
        ["Add-DomainObjectAcl"]    = new[] { "Target", "Principal", "Rights" },
        ["Set-DomainObject"]       = new[] { "Target", "Set" },
    };

    // Operation -> fields that must be filled in (hard requirements).
    private static readonly Dictionary<string, string[]> RequiredFields = new()
    {
        ["Set-DomainUserPassword"] = new[] { "Target" },
        ["Add-DomainGroupMember"]  = new[] { "Target", "Principal" },
        ["Add-DomainObjectAcl"]    = new[] { "Target", "Principal", "Rights" },
        ["Set-DomainObject"]       = new[] { "Target", "Set" },
    };

    private readonly ComboBox _operationBox = new ComboBox();
    private readonly TextBox _targetBox = new TextBox();
    private readonly TextBox _principalBox = new TextBox();
    private readonly TextBox _newPasswordBox = new TextBox();
    private readonly ComboBox _rightsBox = new ComboBox();
    private readonly TextBox _setBox = new TextBox();

    private readonly Dictionary<string, Control> _rows = new();

    public AclTab()
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

        _operationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (string op in Operations)
        {
            _operationBox.Items.Add(op);
        }
        _operationBox.SelectedIndexChanged += OnOperationChanged;
        root.Controls.Add(MakeRow("Operation:", _operationBox));

        _rightsBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _rightsBox.Items.AddRange(new object[] { "All", "ResetPassword", "WriteMembers", "DCSync" });
        _rightsBox.SelectedIndex = 0;

        _newPasswordBox.UseSystemPasswordChar = true;

        root.Controls.Add(TrackedRow("Target", "Target Identity:", _targetBox));
        root.Controls.Add(TrackedRow("Principal", "Principal:", _principalBox));
        root.Controls.Add(TrackedRow("NewPassword", "New Password:", _newPasswordBox));
        root.Controls.Add(TrackedRow("Rights", "Rights:", _rightsBox));
        root.Controls.Add(TrackedRow("Set", "Set (attr=value):", _setBox));

        ToolTip tips = new ToolTip();
        tips.SetToolTip(_operationBox, "The ACL-abuse operation to build.");
        tips.SetToolTip(_targetBox, "The object the operation acts on: the user (Set password), the group (Add member), or the object whose ACL/attribute you change.");
        tips.SetToolTip(_principalBox, "The account that benefits: the user to add to the group, or who receives the ACE.");
        tips.SetToolTip(_newPasswordBox, "The new password for the target user. Only embedded in inline mode.");
        tips.SetToolTip(_rightsBox, "The right to grant. DCSync = replication rights (secrets dump); ResetPassword; WriteMembers; All.");
        tips.SetToolTip(_setBox, "Attribute to set, e.g. serviceprincipalname=nonexistent/svc. Becomes -Set @{'attr'='value'}.");

        Controls.Add(root);

        _operationBox.SelectedIndex = 0;   // triggers OnOperationChanged
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

        if (field is TextBox || field is ComboBox)
        {
            field.Width = 310;
        }
        field.Anchor = AnchorStyles.Left;
        field.Margin = new Padding(3, 4, 3, 3);

        row.Controls.Add(cap, 0, 0);
        row.Controls.Add(field, 1, 0);
        return row;
    }

    private void OnOperationChanged(object? sender, EventArgs e)
    {
        string op = _operationBox.SelectedItem?.ToString() ?? "";
        string[] show = OperationFields.TryGetValue(op, out var f) ? f : Array.Empty<string>();

        foreach (KeyValuePair<string, Control> kv in _rows)
        {
            kv.Value.Visible = Array.IndexOf(show, kv.Key) >= 0;
        }
    }

    public CommandResult Generate(CredentialInput credential)
    {
        string operation = _operationBox.SelectedItem?.ToString() ?? "";
        if (operation.Length == 0)
        {
            return CommandResult.Fail("Please select an ACL operation.");
        }

        string Val(string key, TextBox box)
            => (_rows.TryGetValue(key, out var r) && r.Visible) ? box.Text.Trim() : "";

        string target = Val("Target", _targetBox);
        string principal = Val("Principal", _principalBox);
        string setValue = Val("Set", _setBox);
        string rights = (_rows.TryGetValue("Rights", out var rr) && rr.Visible)
            ? (_rightsBox.SelectedItem?.ToString() ?? "")
            : "";
        string newPassword = _newPasswordBox.Text; // password: not trimmed

        // Credentials are always required for ACL operations.
        string? error = Validation.MissingRequired(
            ("Domain", credential.Domain),
            ("Username", credential.Username));
        if (error != null)
        {
            return CommandResult.Fail(error);
        }

        // Operation-specific required fields.
        if (RequiredFields.TryGetValue(operation, out var required))
        {
            string ValueFor(string key) => key switch
            {
                "Target" => target,
                "Principal" => principal,
                "Rights" => rights,
                "Set" => setValue,
                _ => ""
            };

            List<(string, string)> pairs = new List<(string, string)>();
            foreach (string key in required)
            {
                pairs.Add((LabelFor(key), ValueFor(key)));
            }

            string? reqError = Validation.MissingRequired(pairs.ToArray());
            if (reqError != null)
            {
                return CommandResult.Fail(operation + ": " + reqError);
            }
        }

        // Inline mode only matters for Set-DomainUserPassword (it embeds passwords).
        if (operation == "Set-DomainUserPassword" && !credential.SecureMode
            && (credential.Password.Length == 0 || newPassword.Length == 0))
        {
            return CommandResult.Fail("Inline mode (Credentials tab) needs the Credentials-tab password and a New Password.");
        }

        string script = AclGenerator.Generate(
            operation,
            target,
            principal,
            newPassword,
            rights,
            setValue,
            credential.Domain,
            credential.Username,
            credential.Password,
            credential.SecureMode);

        return CommandResult.Ok(script);
    }

    private static string LabelFor(string key) => key switch
    {
        "Target" => "Target Identity",
        "NewPassword" => "New Password",
        "Set" => "Set",
        _ => key
    };
}
