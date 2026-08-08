using System.Text.Json;
using System.Text.Json.Serialization;

namespace NQuery.Authoring.LanguageServer.Protocol;

// The LSP types below are hand-written rather than taken from a package: the only stable
// protocol-types package (Microsoft.VisualStudio.LanguageServer.Protocol) tops out at 17.2.8
// and is Newtonsoft-based, which would leak into every app-specific host that references this
// library. We only need the subset the server actually implements.
//
// Serialization relies on the JsonSerializerOptions configured in JsonRpcFactory: camelCase
// naming and WhenWritingNull, so optional members are plain nullable properties and only
// members whose wire name differs from camelCase carry a [JsonPropertyName].

// Zero-based line/character offset. Character offsets are UTF-16 code units, which is what
// .NET strings already are -- see PositionEncodingKind in Lifecycle.cs.
public sealed record Position
{
    public required int Line { get; init; }
    public required int Character { get; init; }
}

public sealed record Range
{
    public required Position Start { get; init; }
    public required Position End { get; init; }
}

public sealed record Location
{
    public required Uri Uri { get; init; }
    public required Range Range { get; init; }
}

public sealed record TextDocumentIdentifier
{
    public required Uri Uri { get; init; }
}

public sealed record VersionedTextDocumentIdentifier
{
    public required Uri Uri { get; init; }
    public required int Version { get; init; }
}

public sealed record TextDocumentItem
{
    public required Uri Uri { get; init; }
    public required string LanguageId { get; init; }
    public required int Version { get; init; }
    public required string Text { get; init; }
}

public sealed record TextDocumentPositionParams
{
    public required TextDocumentIdentifier TextDocument { get; init; }
    public required Position Position { get; init; }
}

public sealed record TextEdit
{
    public required Range Range { get; init; }
    public required string NewText { get; init; }
}

public enum MarkupKind
{
    PlainText,
    Markdown
}

public sealed record MarkupContent
{
    public required MarkupKind Kind { get; init; }
    public required string Value { get; init; }

    public static MarkupContent Markdown(string value)
    {
        return new MarkupContent { Kind = MarkupKind.Markdown, Value = value };
    }

    public static MarkupContent PlainText(string value)
    {
        return new MarkupContent { Kind = MarkupKind.PlainText, Value = value };
    }
}

// MarkupKind is one of the few LSP enums that is a string on the wire, not an integer.
internal sealed class MarkupKindConverter : JsonStringEnumConverter<MarkupKind>
{
    public MarkupKindConverter()
        : base(JsonNamingPolicy.CamelCase)
    {
    }
}

// LSP's DocumentUri is always a URI string, but the default converter writes Uri.OriginalString,
// which is the raw constructor argument. A Uri built from a path -- new Uri("/home/me/q.nql") --
// is absolute yet keeps the bare path as its OriginalString, so the default would put a non-URI
// on the wire, and the receiver would parse it back as a *relative* Uri that no longer compares
// equal to the original. Windows hides this: the drive letter in "C:\me\q.nql" makes even the raw
// path parse back as absolute.
internal sealed class UriConverter : JsonConverter<Uri>
{
    public override Uri? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null)
            return null;

        // Relative is still tolerated on the way in: a client that sends a bare path should be
        // understood rather than rejected, even though we never emit one.
        return new Uri(value, UriKind.RelativeOrAbsolute);
    }

    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString);
    }
}
