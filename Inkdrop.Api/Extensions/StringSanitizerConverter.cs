using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Inkdrop.Api.Extensions;

/// <summary>
/// Custom JSON converter that sanitizes all incoming strings to prevent XSS attacks
/// by stripping HTML tags from the input.
/// </summary>
public sealed class StringSanitizerConverter : JsonConverter<string>
{
    // Regex to match any HTML tag (e.g., <script>, <div>, <img />)
    private static readonly Regex HtmlTagRegex = new(
        @"<[^>]*>", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value)) return value ?? string.Empty;

        // Remove all HTML tags to ensure no scripts or malicious HTML are persisted
        return HtmlTagRegex.Replace(value, string.Empty).Trim();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // We don't sanitize on output to avoid double-encoding or modifying data sent back to client
        writer.WriteStringValue(value);
    }
}
