using System.Text.Json;

using NQuery.Authoring.LanguageServer.Protocol;

namespace NQuery.Authoring.LanguageServer.Hosting;

// What an app-specific catalog provider can do to the editor while it resolves: report progress,
// ask a question, read client-side configuration. Everything here is standard LSP, so a host
// built against it works in any client, not just the VS Code extension.
public interface ILanguageServerHost
{
    Task ShowMessageAsync(MessageType type, string message, CancellationToken cancellationToken = default);

    Task<MessageActionItem?> ShowMessageRequestAsync(MessageType type,
                                                     string message,
                                                     IReadOnlyList<MessageActionItem> actions,
                                                     CancellationToken cancellationToken = default);

    Task LogAsync(MessageType type, string message, CancellationToken cancellationToken = default);

    // Pulls a settings section from the client (workspace/configuration). Returns null when the
    // client does not support the request or has nothing for that section.
    Task<JsonElement?> GetConfigurationAsync(string section, CancellationToken cancellationToken = default);
}
