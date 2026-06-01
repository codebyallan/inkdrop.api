using System.Text.RegularExpressions;

namespace Inkdrop.Api.Extensions;

public static class InputValidator
{
    // Matches common XSS patterns that might bypass simple tag stripping
    // e.g., "javascript:", "data:", "vbscript:", "onload=", "onerror="
    private static readonly Regex MaliciousPatternRegex = new(
        @"(javascript:|data:|vbscript:|onload=|onerror=|alert\(|confirm\(|prompt\()", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool IsSafe(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return true;
        return !MaliciousPatternRegex.IsMatch(input);
    }
}
