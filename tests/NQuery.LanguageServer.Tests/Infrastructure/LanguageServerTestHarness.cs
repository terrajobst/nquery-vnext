using System.Text.Json;

using Nerdbank.Streams;

using NQuery.Authoring.LanguageServer;
using NQuery.Authoring.LanguageServer.Hosting;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.Authoring.LanguageServer.Server;

using StreamJsonRpc;

using LspDiagnostic = NQuery.Authoring.LanguageServer.Protocol.Diagnostic;

namespace NQuery.LanguageServer.Infrastructure;

// Runs a real server over an in-memory duplex pair -- same JSON-RPC framing, serialization and
// dispatch as a child process, without the process. Out-of-process coverage lives in
// NorthwindHostProcessTests.
internal sealed class LanguageServerTestHarness : IAsyncDisposable
{
    private readonly JsonRpc _client;
    private readonly Task _serverTask;
    private readonly ClientTarget _target;

    private LanguageServerTestHarness(JsonRpc client, Task serverTask, ClientTarget target)
    {
        _client = client;
        _serverTask = serverTask;
        _target = target;
    }

    public static async Task<LanguageServerTestHarness> StartAsync(Catalog? catalog = null,
                                                                   object? settings = null,
                                                                   TimeSpan catalogDelay = default,
                                                                   Action<NQueryLanguageServerOptions>? configure = null,
                                                                   ICatalogProvider? catalogProvider = null)
    {
        var (serverStream, clientStream) = FullDuplexStream.CreatePair();

        var server = NQueryLanguageServer.Create(options =>
        {
            options.CatalogProviderFactory = _ => catalogProvider ?? new FixedCatalogProvider(catalog ?? Catalog.Default, catalogDelay);

            // Tests drive changes explicitly and then wait for the publish; a debounce would only
            // add latency and flakiness.
            options.DiagnosticsDelay = TimeSpan.Zero;

            configure?.Invoke(options);
        });

        var serverTask = server.RunAsync(serverStream, serverStream);

        var target = new ClientTarget();
        var formatter = new SystemTextJsonFormatter { JsonSerializerOptions = LspJson.CreateOptions() };
        var handler = new HeaderDelimitedMessageHandler(clientStream, clientStream, formatter);
        var client = new JsonRpc(handler);
        client.AddLocalRpcTarget(target);
        client.StartListening();

        var harness = new LanguageServerTestHarness(client, serverTask, target);
        await harness.InitializeAsync(settings);
        return harness;
    }

    public InitializeResult InitializeResult { get; private set; } = null!;

    private async Task InitializeAsync(object? settings)
    {
        var initializationOptions = JsonSerializer.SerializeToElement(
            new { projectName = @"Test", settings },
            LspJson.CreateOptions());

        var parameters = new InitializeParams
        {
            ProcessId = Environment.ProcessId,
            ClientInfo = new ClientInfo { Name = @"NQuery.LanguageServer.Tests" },
            InitializationOptions = initializationOptions,
            Capabilities = new ClientCapabilities()
        };

        InitializeResult = await _client.InvokeWithParameterObjectAsync<InitializeResult>(Methods.Initialize, parameters);
        await _client.NotifyWithParameterObjectAsync(Methods.Initialized, new { });
    }

    // Registered before the triggering notification is sent, so a publish can never be missed.
    public Task<IReadOnlyList<LspDiagnostic>> ExpectDiagnostics(Uri uri)
    {
        return _target.Expect(uri);
    }

    /// Every window/showMessage the server has sent so far.
    public IReadOnlyList<ShowMessageParams> ShownMessages
    {
        get { return _target.ShownMessages; }
    }

    public Task<ShowMessageParams> ExpectShowMessage()
    {
        return _target.ExpectShowMessage();
    }

    public Task OpenAsync(Uri uri, string text, int version = 1)
    {
        var parameters = new DidOpenTextDocumentParams
        {
            TextDocument = new TextDocumentItem
            {
                Uri = uri,
                LanguageId = @"nquery",
                Version = version,
                Text = text
            }
        };

        return _client.NotifyWithParameterObjectAsync(Methods.TextDocumentDidOpen, parameters);
    }

    public Task ChangeAsync(Uri uri, int version, params TextDocumentContentChangeEvent[] changes)
    {
        var parameters = new DidChangeTextDocumentParams
        {
            TextDocument = new VersionedTextDocumentIdentifier { Uri = uri, Version = version },
            ContentChanges = changes
        };

        return _client.NotifyWithParameterObjectAsync(Methods.TextDocumentDidChange, parameters);
    }

    public Task CloseAsync(Uri uri)
    {
        var parameters = new DidCloseTextDocumentParams
        {
            TextDocument = new TextDocumentIdentifier { Uri = uri }
        };

        return _client.NotifyWithParameterObjectAsync(Methods.TextDocumentDidClose, parameters);
    }

    public Task<T> RequestAsync<T>(string method, object parameters)
    {
        return _client.InvokeWithParameterObjectAsync<T>(method, parameters);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _client.InvokeAsync<object?>(Methods.Shutdown);
            await _client.NotifyAsync(Methods.Exit);
            await _serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (Exception)
        {
            // The connection may already be torn down; the test result is what matters.
        }
        finally
        {
            _client.Dispose();
        }
    }

    private sealed class ClientTarget
    {
        private readonly Dictionary<Uri, TaskCompletionSource<IReadOnlyList<LspDiagnostic>>> _waiters = new();
        private readonly List<ShowMessageParams> _shownMessages = new();
        private readonly List<TaskCompletionSource<ShowMessageParams>> _messageWaiters = new();
        private readonly object _gate = new();

        public IReadOnlyList<ShowMessageParams> ShownMessages
        {
            get
            {
                lock (_gate)
                    return _shownMessages.ToArray();
            }
        }

        public Task<IReadOnlyList<LspDiagnostic>> Expect(Uri uri)
        {
            lock (_gate)
            {
                var source = new TaskCompletionSource<IReadOnlyList<LspDiagnostic>>(TaskCreationOptions.RunContinuationsAsynchronously);
                _waiters[uri] = source;
                return source.Task;
            }
        }

        public Task<ShowMessageParams> ExpectShowMessage()
        {
            lock (_gate)
            {
                // Catalog resolution starts at `initialized`, so the message may already have
                // arrived before a test gets a chance to wait for it.
                if (_shownMessages.Count > 0)
                    return Task.FromResult(_shownMessages[0]);

                var source = new TaskCompletionSource<ShowMessageParams>(TaskCreationOptions.RunContinuationsAsynchronously);
                _messageWaiters.Add(source);
                return source.Task;
            }
        }

        [JsonRpcMethod(Methods.TextDocumentPublishDiagnostics, UseSingleObjectParameterDeserialization = true)]
        public void PublishDiagnostics(PublishDiagnosticsParams parameters)
        {
            lock (_gate)
            {
                if (_waiters.Remove(parameters.Uri, out var source))
                    source.TrySetResult(parameters.Diagnostics);
            }
        }

        [JsonRpcMethod(Methods.WindowLogMessage, UseSingleObjectParameterDeserialization = true)]
        public void LogMessage(LogMessageParams parameters)
        {
        }

        [JsonRpcMethod(Methods.WindowShowMessage, UseSingleObjectParameterDeserialization = true)]
        public void ShowMessage(ShowMessageParams parameters)
        {
            TaskCompletionSource<ShowMessageParams>[] waiters;

            lock (_gate)
            {
                _shownMessages.Add(parameters);
                waiters = _messageWaiters.ToArray();
                _messageWaiters.Clear();
            }

            foreach (var waiter in waiters)
                waiter.TrySetResult(parameters);
        }
    }
}
