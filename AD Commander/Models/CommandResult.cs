namespace AD_Commander.Models;

// The outcome of a Generate call: either the generated PowerShell text, or an error to show.
public sealed class CommandResult
{
    public bool Success { get; }
    public string Text { get; }
    public string Error { get; }

    private CommandResult(bool success, string text, string error)
    {
        Success = success;
        Text = text;
        Error = error;
    }

    public static CommandResult Ok(string text) => new CommandResult(true, text, "");

    public static CommandResult Fail(string error) => new CommandResult(false, "", error);
}
