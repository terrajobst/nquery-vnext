# Language Server Protocol

`NQuery.Authoring.LanguageServer` exposes the authoring services over the Language Server
Protocol. It is an editor adapter in the same sense as `NQuery.Authoring.ActiproWpf` and
`NQuery.Authoring.VSEditorWpf` — it maps the models in `NQuery.Authoring` onto a specific
editor surface, and contains no language logic of its own.

The difference is that LSP is a process boundary rather than an in-process API, and NQuery's
catalog is defined in code. So there is no single NQuery language server that could be shipped:
every application builds its own host executable, and the VS Code extension launches whichever
one a project file points at.

---

## Projects

| Project                            | Purpose                                                |
| ---------------------------------- | ------------------------------------------------------ |
| `NQuery.Authoring.LanguageServer`  | The server library (net8.0). Referenced by app hosts.  |
| `NQuery.LanguageServer.Northwind`  | Reference/test host over `NorthwindCatalog`.           |
| `NQuery.LanguageServer.Tests`      | In-process and out-of-process server tests.            |
| `vscode`                            | The VS Code client.                                    |

The only external dependency is `StreamJsonRpc`. The LSP types are hand-written under
`Protocol/`: the one stable protocol-types package (`Microsoft.VisualStudio.LanguageServer.Protocol`)
is from 2022 and Newtonsoft-based, which would leak into every host that references this library.

---

## Writing a host

```csharp
await NQueryLanguageServer.Create(options =>
    {
        options.ServerName = "Warehouse Language Server";
        options.CatalogProviderFactory = context => new WarehouseCatalogProvider(context);
    })
    .RunAsync(Console.OpenStandardInput(), Console.OpenStandardOutput());
```

`CatalogProviderFactory` is a factory rather than an instance because the project's settings only
arrive with `initialize`, after the process has started.

```csharp
public interface ICatalogProvider
{
    ValueTask<Catalog> GetCatalogAsync(CancellationToken cancellationToken = default);
    event EventHandler<EventArgs>? CatalogChanged;
}
```

Resolution is async because a real catalog usually comes from somewhere slow. Requests that
arrive while it is still loading wait for it rather than being answered against `Catalog.Empty`.

`CatalogChanged` re-resolves the catalog and re-publishes diagnostics for every open document.

### When resolution fails

The server stays up and degrades to **syntax-only**, which means specifically:

- Diagnostics come from the syntax tree alone, plus a single `CatalogUnavailable` error naming
  the underlying failure. Binding is skipped entirely — without a catalog every table and column
  is undeclared, and reporting each one buries the real cause under dozens of symptoms.
- `nquery/execute` and `nquery/showPlan` report the catalog failure rather than letting the
  compiler complain that nothing is declared.
- The failure is announced three ways, because a `window/showMessage` toast alone is far too easy
  to miss: the toast, the persistent `CatalogUnavailable` diagnostic, and an
  `nquery/catalogStatus` notification the client renders in the status bar.

`nquery/reloadCatalog` retries and clears the state.

### Talking back to the editor

`ProjectContext.Host` is an `ILanguageServerHost`:

```csharp
Task ShowMessageAsync(MessageType type, string message, CancellationToken ct = default);
Task<MessageActionItem?> ShowMessageRequestAsync(MessageType type, string message,
                                                 IReadOnlyList<MessageActionItem> actions,
                                                 CancellationToken ct = default);
Task LogAsync(MessageType type, string message, CancellationToken ct = default);
Task<JsonElement?> GetConfigurationAsync(string section, CancellationToken ct = default);
```

All four are standard LSP, so a host built against them works in any client, not only VS Code.

> Anything the host writes to `Console.Out` corrupts the message stream. The Northwind host
> repoints `Console.Out` at stderr before starting the server; real hosts should do the same.

---

## Feature mapping

Every feature delegates to the existing authoring APIs:

| LSP request           | Authoring API                                          |
| --------------------- | ------------------------------------------------------ |
| `publishDiagnostics`  | `SemanticModel.GetDiagnostics()` + `GetIssues()`       |
| `completion`          | `GetCompletionModel(position)`                          |
| `hover`               | `GetQuickInfoModel(position)`                           |
| `signatureHelp`       | `GetSignatureHelpModel(position)`                       |
| `definition`          | `FindSymbol` + `FindUsages` (definitions only)          |
| `references`          | `FindUsages`                                            |
| `documentHighlight`   | `GetHighlights(position)`                               |
| `semanticTokens/full` | `ClassifySyntax()` + `ClassifySemantics(model)`         |
| `foldingRange`        | `Root.FindRegions()`                                    |
| `selectionRange`      | `SyntaxTree.ExtendSelection(span)`, walked to a fixpoint |
| `codeAction`          | `GetFixes(pos)`, `GetRefactorings(pos)`, `CodeIssue.Actions`  |

Notes:

- **Diagnostics.** `Diagnostic` carries no severity, so everything the compiler reports maps to
  `Error`; `CodeIssue` is the only source distinguishing levels. `DiagnosticId` is surfaced as
  the LSP `code`.
- **Completion.** Items always carry an explicit `textEdit` built from the model's
  `ApplicableSpan`, rather than relying on the client's word-boundary guess. That is what makes
  bracketed identifiers such as `[Order Details]` replace correctly.
- **Signature help.** `ParameterItem.Span` indexes into the signature's own text, which maps
  directly onto LSP's offset-pair parameter label — no substring matching.
- **Semantic tokens.** Syntax classification supplies the non-identifier tokens; identifiers are
  left to the semantic pass, so an identifier that binds to nothing emits no token and falls
  through to the TextMate grammar. Multi-line spans (block comments) are split per line, which
  LSP requires.
- **Positions.** LSP character offsets are UTF-16 code units, which is what `SourceText` already
  indexes by, so no encoding conversion happens. The server advertises `positionEncoding: utf-16`.
- **Text sync.** LSP applies the changes in one `didChange` in order, each against the result of
  the previous one. `SourceText.WithChanges` applies a batch against a single snapshot, so
  `LspSourceTextContainer` deliberately calls it once per change.

---

## Running queries and show plan

Two non-standard requests, advertised under `ServerCapabilities.Experimental` so a client can
discover them rather than guess:

| Request            | Returns                                                        |
| ------------------ | -------------------------------------------------------------- |
| `nquery/execute`   | Columns, rows, elapsed time, truncation flag, or an error       |
| `nquery/showPlan`  | The full optimization pipeline, or an error                     |

Both work for `.nql` and `.nqe` alike: `Compilation.Compile()` handles a bare expression by
wrapping it in a one-row projection, so there is a single execution path.

```csharp
public bool AllowExecution { get; set; } = true;      // hosts over production data should clear this
public int MaxRows { get; set; } = 1000;              // a client may request fewer, never more
public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);
```

Notes:

- **Cells cross the wire as display strings**, with JSON `null` for SQL NULL. Typed JSON would
  round decimals through doubles and turn a `byte[]` column — Northwind's `Categories.Picture`
  carries real image data — into megabytes of base64. Binary renders as `byte[1234]`.
- **Execution runs on the thread pool.** Iterators are synchronous and blocking, so the rest of
  the server keeps answering requests. Cancellation is checked once per row; a single row that
  blocks forever cannot be interrupted.
- **Compilation failure is a result, not a fault.** `Compile()` throws `CompilationException`
  when the query has diagnostics; that becomes `ErrorMessage` so the panel stays
  self-explanatory.
- **Show plan is the whole pipeline** — `GetShowPlanSteps()` yields the unoptimized logical tree,
  one entry per optimization pass that changed it, the optimized tree, and the physical plan.
  The client defaults to the last.
- **`ShowPlanNode.Properties` is usually empty.** Detail is encoded in `OperatorName`
  (`Table (Customers), DefinedValues := c.CompanyName:1`), so a renderer must lead with the name
  and treat properties as optional. `IsScalar` marks expression subtrees.

### Code actions

`textDocument/codeAction` serves all three provider families: `GetFixes` and the actions carried
by each `CodeIssue` as `quickfix`, and `GetRefactorings` as `refactor`.

Turning an action into edits needs no new API. `ICodeAction.GetEdit()` returns the rewritten
`SyntaxTree`, and because `SyntaxTree.WithChanges` goes through `SourceText.WithChanges`, that
tree's text is a `ChangedSourceText` chained to the original. `newTree.Text.GetChanges(oldText)`
walks that chain and returns the exact `TextChangeSet` the action built — it does not diff. So the
edits are precise and the user's cursor and folding survive.

Edits are computed for every offered action rather than through `codeAction/resolve`, which costs
one reparse per action each time the lightbulb is consulted. That is comfortable at the size of a
query; `resolveProvider` is the lever if it ever stops being.

### Not implemented

- **Formatting** — there is no formatter in the codebase.
- **Rename** — belongs in the authoring layer rather than here: `SymbolSearcher` supplies the
  spans, but conflict detection and re-quoting a name that needs brackets are language concerns.
- **Brace matching and commenting** — handled client-side by
  `language-configuration.json`.

---

## Concurrency

`DocumentStore` serializes access with a plain lock, held only long enough to swap text or take a
`Document` snapshot — never across compilation. `Document` is immutable and computes its syntax
tree and semantic model behind `Interlocked`, so a handler can do the expensive work off the lock.

---

## Testing

```
dotnet test --project tests/NQuery.LanguageServer.Tests/NQuery.LanguageServer.Tests.csproj
```

Two layers:

- **In-process** — a real server over an in-memory duplex pair, so framing, serialization and
  dispatch are all exercised without a child process. Most tests live here.
- **Out-of-process** — launches the real Northwind host over stdio, covering startup, the
  `initializationOptions` round trip, and shutdown.

`SampleWorkspaceTests` runs every file in `samples/northwind` through the server and fails if any
produces an error, so the sample workspace cannot rot.

The VS Code client has its own suites under `vscode/tests`:

```
cd vscode
npm test                  # unit, then integration
```

- **Unit** (`node:test`) — the project ownership rules and webview HTML, both kept free of any
  `vscode` dependency so they can be called directly.
- **Integration** (`@vscode/test-cli`) — a real VS Code instance driving the real Northwind host,
  covering discovery, launching, and diagnostics arriving end to end. Build the host first.

From the repository root, `build.cmd` and `test.cmd` cover both stacks; `test.cmd -full` adds the
integration tests, which are otherwise opt-in locally because the first run downloads VS Code.
