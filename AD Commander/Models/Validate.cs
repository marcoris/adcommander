using System.Collections.Generic;

namespace AD_Commander.Models;

// Tiny shared input validator. Collects the labels of required fields that are blank and builds
// one message, so a tab can report every missing field at once instead of one at a time.
public static class Validation
{
    // Returns null if every (label, value) pair has a non-blank value;
    // otherwise "Please fill in: Label1, Label2".
    public static string? MissingRequired(params (string Label, string Value)[] fields)
    {
        List<string> missing = new List<string>();
        foreach ((string label, string value) in fields)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                missing.Add(label);
            }
        }

        return missing.Count == 0
            ? null
            : "Please fill in: " + string.Join(", ", missing);
    }
}
