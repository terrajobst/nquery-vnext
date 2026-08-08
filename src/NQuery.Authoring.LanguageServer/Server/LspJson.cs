using System.Text.Json;
using System.Text.Json.Serialization;

using NQuery.Authoring.LanguageServer.Protocol;

namespace NQuery.Authoring.LanguageServer.Server;

internal static class LspJson
{
    // LSP is camelCase everywhere and omits absent members rather than sending null, so both are
    // configured globally instead of per-property. Enums stay numeric (the protocol default);
    // MarkupKind is the exception and gets an explicit string converter. Uri gets one too, so
    // document URIs go out as URIs rather than as whatever string built them.
    public static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new MarkupKindConverter(), new UriConverter() }
        };
    }
}
