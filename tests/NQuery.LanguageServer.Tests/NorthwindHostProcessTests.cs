using System.Diagnostics;
using System.Text.Json;

using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.Authoring.LanguageServer.Server;

using StreamJsonRpc;

using LspDiagnostic = NQuery.Authoring.LanguageServer.Protocol.Diagnostic;

namespace NQuery.LanguageServer;

// The in-process tests cover the handlers; these cover the things only a real process can get
// wrong -- launching, the initializationOptions round trip, stdout staying clean, and shutdown.
[Trait(@"Category", @"OutOfProcess")]
public sealed class NorthwindHostProcessTests
{
    private static readonly Uri DocumentUri = new(@"file:///c:/queries/process.nql");

    [Fact]
    public async Task Host_ServesDiagnosticsOverStdio()
    {
        await using var host = await NorthwindHost.StartAsync();

        var expectation = host.ExpectDiagnostics(DocumentUri);
        await host.OpenAsync(DocumentUri, @"SELECT * FROM Bogus");
        var diagnostics = await expectation.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        Assert.Contains(diagnostics, d => d.Code == @"UndeclaredTable");
    }

    [Fact]
    public async Task Host_UsesNorthwindCatalog()
    {
        await using var host = await NorthwindHost.StartAsync();

        var expectation = host.ExpectDiagnostics(DocumentUri);
        await host.OpenAsync(DocumentUri, @"SELECT c.CompanyName FROM Customers c");
        var diagnostics = await expectation.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task Host_ForwardsSettingsBlob()
    {
        // failWith drives the app-specific provider through its failure path; the server must
        // stay up and degrade to syntax-only rather than dying with it.
        await using var host = await NorthwindHost.StartAsync(new { failWith = @"simulated outage" });

        var expectation = host.ExpectDiagnostics(DocumentUri);
        await host.OpenAsync(DocumentUri, @"SELECT * FROM Customers");
        var diagnostics = await expectation.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // One diagnostic naming the real cause -- not "Customers is undeclared", which is a
        // symptom of the missing catalog rather than a problem with the query.
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(@"CatalogUnavailable", diagnostic.Code);
        Assert.Contains(@"simulated outage", diagnostic.Message);
    }

    [Fact]
    public async Task Host_ReportsServerInfo()
    {
        await using var host = await NorthwindHost.StartAsync();

        Assert.Equal(@"NQuery Northwind Language Server", host.InitializeResult.ServerInfo?.Name);
    }

    private sealed class NorthwindHost : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly JsonRpc _client;
        private readonly Dictionary<Uri, TaskCompletionSource<IReadOnlyList<LspDiagnostic>>> _waiters = new();
        private readonly Lock _gate = new();

        private NorthwindHost(Process process, JsonRpc client)
        {
            _process = process;
            _client = client;
        }

        public InitializeResult InitializeResult { get; private set; } = null!;

        public static async Task<NorthwindHost> StartAsync(object? settings = null)
        {
            var dll = FindHostAssembly();

            var startInfo = new ProcessStartInfo(@"dotnet")
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(dll);

            var process = Process.Start(startInfo) ?? throw new InvalidOperationException(@"Could not start the host process.");
            process.ErrorDataReceived += (_, _) => { };
            process.BeginErrorReadLine();

            var options = LspJson.CreateOptions();
            var formatter = new SystemTextJsonFormatter { JsonSerializerOptions = options };
            var handler = new HeaderDelimitedMessageHandler(process.StandardInput.BaseStream,
                                                            process.StandardOutput.BaseStream,
                                                            formatter);

            var host = new NorthwindHost(process, new JsonRpc(handler));
            host._client.AddLocalRpcTarget(new ClientTarget(host.OnDiagnostics));
            host._client.StartListening();

            var initializationOptions = JsonSerializer.SerializeToElement(
                new { projectName = @"Northwind", settings }, options);

            host.InitializeResult = await host._client.InvokeWithParameterObjectAsync<InitializeResult>(
                Methods.Initialize,
                new InitializeParams
                {
                    ProcessId = Environment.ProcessId,
                    ClientInfo = new ClientInfo { Name = @"NQuery.LanguageServer.Tests" },
                    InitializationOptions = initializationOptions,
                    Capabilities = new ClientCapabilities()
                }).WaitAsync(TimeSpan.FromSeconds(60));

            await host._client.NotifyWithParameterObjectAsync(Methods.Initialized, new { });
            return host;
        }

        public Task<IReadOnlyList<LspDiagnostic>> ExpectDiagnostics(Uri uri)
        {
            lock (_gate)
            {
                var source = new TaskCompletionSource<IReadOnlyList<LspDiagnostic>>(TaskCreationOptions.RunContinuationsAsynchronously);
                _waiters[uri] = source;
                return source.Task;
            }
        }

        private void OnDiagnostics(PublishDiagnosticsParams parameters)
        {
            lock (_gate)
            {
                if (_waiters.Remove(parameters.Uri, out var source))
                    source.TrySetResult(parameters.Diagnostics);
            }
        }

        public Task OpenAsync(Uri uri, string text)
        {
            return _client.NotifyWithParameterObjectAsync(Methods.TextDocumentDidOpen, new DidOpenTextDocumentParams
            {
                TextDocument = new TextDocumentItem
                {
                    Uri = uri,
                    LanguageId = @"nquery",
                    Version = 1,
                    Text = text
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _client.InvokeAsync<object?>(Methods.Shutdown).WaitAsync(TimeSpan.FromSeconds(10));
                await _client.NotifyAsync(Methods.Exit);

                if (!_process.WaitForExit(TimeSpan.FromSeconds(10)))
                    _process.Kill(entireProcessTree: true);
            }
            catch (Exception)
            {
                try
                {
                    _process.Kill(entireProcessTree: true);
                }
                catch (Exception)
                {
                    // Already gone.
                }
            }
            finally
            {
                _client.Dispose();
                _process.Dispose();
            }
        }

        // Both projects land under artifacts/bin/<Project>/debug, so the host is a sibling of the
        // test binary's own output directory.
        private static string FindHostAssembly()
        {
            const string name = @"NQuery.LanguageServer.Northwind";

            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, name, @"debug", $"{name}.dll");
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            throw new FileNotFoundException($"Could not locate {name}.dll. Build the host project first.");
        }

        private sealed class ClientTarget
        {
            private readonly Action<PublishDiagnosticsParams> _onDiagnostics;

            public ClientTarget(Action<PublishDiagnosticsParams> onDiagnostics)
            {
                _onDiagnostics = onDiagnostics;
            }

            [JsonRpcMethod(Methods.TextDocumentPublishDiagnostics, UseSingleObjectParameterDeserialization = true)]
            public void PublishDiagnostics(PublishDiagnosticsParams parameters)
            {
                _onDiagnostics(parameters);
            }

            [JsonRpcMethod(Methods.WindowLogMessage, UseSingleObjectParameterDeserialization = true)]
            public void LogMessage(LogMessageParams parameters)
            {
            }

            [JsonRpcMethod(Methods.WindowShowMessage, UseSingleObjectParameterDeserialization = true)]
            public void ShowMessage(ShowMessageParams parameters)
            {
            }
        }
    }
}
