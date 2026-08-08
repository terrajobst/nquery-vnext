using NQuery.Authoring.LanguageServer.Server;

using StreamJsonRpc;

namespace NQuery.Authoring.LanguageServer;

// Entry point for an app-specific host:
//
//     await NQueryLanguageServer.Create(options =>
//         {
//             options.CatalogProviderFactory = ctx => new MyCatalogProvider(ctx);
//         })
//         .RunAsync(Console.OpenStandardInput(), Console.OpenStandardOutput());
//
public sealed class NQueryLanguageServer
{
    private readonly NQueryLanguageServerOptions _options;

    private NQueryLanguageServer(NQueryLanguageServerOptions options)
    {
        _options = options;
    }

    public static NQueryLanguageServer Create(Action<NQueryLanguageServerOptions>? configure = null)
    {
        var options = new NQueryLanguageServerOptions();
        configure?.Invoke(options);
        return new NQueryLanguageServer(options);
    }

    public static NQueryLanguageServer Create(NQueryLanguageServerOptions options)
    {
        ThrowIfNull(options);

        return new NQueryLanguageServer(options);
    }

    // Runs until the client sends `exit` or the connection drops.
    public async Task RunAsync(Stream input, Stream output, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(input);
        ThrowIfNull(output);

        var target = new LanguageServerTarget(_options);

        // LSP frames messages with Content-Length headers, which is what
        // HeaderDelimitedMessageHandler speaks.
        var formatter = new SystemTextJsonFormatter { JsonSerializerOptions = LspJson.CreateOptions() };
        var handler = new HeaderDelimitedMessageHandler(output, input, formatter);

        using var rpc = new JsonRpc(handler);
        rpc.AddLocalRpcTarget(target, new JsonRpcTargetOptions { AllowNonPublicInvocation = false });
        target.Attach(rpc);
        rpc.StartListening();

        // `exit` completes Exited; a dropped connection completes rpc.Completion. Either ends the
        // process, and rpc.Completion faulting on disconnect is a normal shutdown here, not an error.
        var completion = rpc.Completion;
        var exited = target.Exited;
        var cancelled = cancellationToken.CanBeCanceled
            ? Task.Delay(Timeout.Infinite, cancellationToken)
            : Task.Delay(Timeout.Infinite, CancellationToken.None);

        var finished = await Task.WhenAny(completion, exited, cancelled);

        if (ReferenceEquals(finished, completion))
        {
            try
            {
                await completion;
            }
            catch (Exception) when (completion.IsFaulted)
            {
                // The client went away without a clean shutdown/exit handshake.
            }
        }
    }
}
