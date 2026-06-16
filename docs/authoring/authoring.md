# Authoring

The `NQuery.Authoring` project provides editor services on top of the NQuery
compiler, enabling rich IDE features in any text editor. It follows a
**provider-based architecture**: every feature is defined by an interface, with
multiple `I*Provider` implementations that are aggregated together.

The library is editor-agnostic — it operates on `SyntaxTree`, `SemanticModel`,
and `SourceText` without any dependency on a specific editor framework.
Editor-specific integrations are in separate projects
(`NQuery.Authoring.VSEditorWpf`, `NQuery.Authoring.ActiproWpf`).

---

## Projects

| Project                        | Purpose                                                                                                       |
| ------------------------------ | ------------------------------------------------------------------------------------------------------------- |
| `NQuery.Authoring`             | Core editor-agnostic services (models, providers, workspace, document)                                        |
| `NQuery.Authoring.Composition` | MEF-based composition that aggregates providers                                                               |
| `NQuery.Authoring.Wpf`         | Shared WPF UI components (glyphs, code action popup, diagnostic grid, show plan view, syntax tree visualizer) |
| `NQuery.Authoring.VSEditorWpf` | Visual Studio Editor integration                                                                              |
| `NQuery.Authoring.ActiproWpf`  | Actipro SyntaxEditor integration                                                                              |
| `NQuery.Authoring.Tests`       | Tests for all authoring services                                                                              |

---

## Workspace and Document Model

### Workspace

The `Workspace` class tracks the current document state in response to text
changes. It wraps a `SourceTextContainer` and lazily updates a `Document` when
the text changes.

```csharp
public sealed class Workspace
{
    public Workspace(SourceTextContainer textContainer);

    public Document CurrentDocument { get; }
    public DocumentKind DocumentKind { get; set; }
    public Catalog Catalog { get; set; }
    public SourceTextContainer TextContainer { get; }

    public event EventHandler<EventArgs> CurrentDocumentChanged;
}
```

### Document

The `Document` class is the unit of compilation. It owns a `SourceText` and
lazily computes the `SyntaxTree`, `Compilation`, and `SemanticModel` on demand
via async methods with caching.

```csharp
public sealed class Document
{
    public DocumentKind Kind { get; }
    public Catalog Catalog { get; }
    public SourceText Text { get; }

    // Synchronous access (returns true if already computed)
    public bool TryGetSyntaxTree(out SyntaxTree syntaxTree);
    public bool TryGetCompilation(out Compilation compilation);
    public bool TryGetSemanticModel(out SemanticModel semanticModel);

    // Async computation with caching
    public Task<SyntaxTree> GetSyntaxTreeAsync(CancellationToken cancellationToken = default);
    public Task<Compilation> GetCompilationAsync(CancellationToken cancellationToken = default);
    public Task<SemanticModel> GetSemanticModelAsync(CancellationToken cancellationToken = default);
}
```

`DocumentKind` and `Catalog` can be changed by the workspace to support
different document types (e.g., query files vs. expression files) and schema
contexts.

### DocumentView

`DocumentView` is a snapshot of the editor state at a point in time, combining
the document with caret position and selection span:

```csharp
public sealed class DocumentView
{
    public Document Document { get; }
    public int Position { get; }
    public TextSpan Selection { get; }
}
```

---

## Services

### Classification

Two levels of classification:

- **Syntax classification** (`SyntaxClassificationWorker`): Classifies tokens by
  their syntax kind (keyword, identifier, string literal, number, comment,
  operator, punctuation, whitespace). Produces `SyntaxClassificationSpan`
  values.
- **Semantic classification** (`SemanticClassificationWorker`): Classifies
  identifiers by their resolved symbol type (table, column, function, aggregate,
  method, property, variable, parameter). Produces `SemanticClassificationSpan`
  values.

### Brace Matching

The `BraceMatcher` base class defines pairs of matching syntax tokens. Built-in
matchers:

- Parentheses `(`, `)`
- Brackets `[`, `]`
- `CASE` / `END`
- `BEGIN` / `END` (for subqueries)

```csharp
public abstract class BraceMatcher
{
    public abstract ImmutableArray<SyntaxKind> Tokens { get; }
    public abstract TextSpan? GetMatchingSpan(SyntaxTree syntaxTree, int position);
}
```

The `BraceMatcherService` (in `NQuery.Authoring.Composition`) aggregates all
MEF-imported matchers with the built-in ones.

### Completion

The completion system provides IntelliSense-style statement completion. It
consists of:

- **`CompletionProvider`**: Abstract base class for completion providers. Each
  provider returns `CompletionItem` values for a given position.
- **`CompletionModel`**: The completed result — a list of `CompletionItem`
  values.

Built-in completion providers:

- `KeywordCompletionProvider` — SQL keywords
- `FunctionCompletionProvider` — Function symbols
- `AggregateCompletionProvider` — Aggregate symbols
- `TableCompletionProvider` — Table and derived table references
- `ColumnCompletionProvider` — Column names in context
- `VariableCompletionProvider` — Variables and parameters
- `NamedTableReferenceCompletionProvider` — Table names in FROM clause

### Quick Info

The quick info system provides tooltip information (type and signature) for
symbols at a position.

- **`IQuickInfoModelProvider`**: Interface for quick info providers.
- **`QuickInfoModel`**: The quick info result — a `TextSpan` (the relevant span)
  and a `SymbolMarkup` (formatted content).

Built-in quick info providers:

- `TableQuickInfoModelProvider`
- `ColumnQuickInfoModelProvider`
- `FunctionQuickInfoModelProvider`
- `AggregateQuickInfoModelProvider`
- `MethodQuickInfoModelProvider`
- `PropertyQuickInfoModelProvider`
- `VariableQuickInfoModelProvider`
- `ParameterQuickInfoModelProvider`

### Signature Help

The signature help system provides parameter info for function and method
invocations.

- **`ISignatureHelpModelProvider`**: Interface for signature help providers.
- **`SignatureHelpModel`**: The result — a list of `SignatureItem` objects, each
  with `Parameters`, `Description`, and optional `Documentation`.
- **`SignatureItem`**: A single overload with its `Parameters`.
- **`ParameterItem`**: A single parameter with name, description, and type info.

Built-in signature help providers:

- `FunctionSignatureHelpModelProvider`
- `AggregateSignatureHelpModelProvider`
- `MethodSignatureHelpModelProvider`

### Highlighting

The highlighting system finds all references to a symbol at a given position,
enabling reference highlighting in the editor.

- **`IHighlighter`**: Interface for highlighters. Finds all spans that reference
  the same symbol as the one at the given position.
- **`SelectQueryKeywordHighlighterBase`**: Base class for keyword-based
  highlighters (e.g., highlighting all parts of a SELECT statement).

Built-in highlighters:

- `TableHighlighter`
- `ColumnHighlighter`
- `FunctionHighlighter`
- `AggregateHighlighter`
- `MethodHighlighter`
- `PropertyHighlighter`
- `VariableHighlighter`
- `ParameterHighlighter`
- `SelectQueryKeywordHighlighter` — Highlights matching `SELECT`/`FROM`/`WHERE`
  keywords

### Outlining

The outlining system provides collapsible regions in the editor.

- **`OutliningWorker`**: Walks the syntax tree and produces outfolding regions.
- **`OutliningRegionSpan`**: A span with text to display when collapsed.
- **`SyntaxNodeOutliner`**: Base class for outliners based on syntax node types.
- **`SyntaxTokenOutliner`**: Base class for outliners based on syntax token
  types.

Built-in outliners collapse: multi-line comments, subqueries, CTE definitions,
join specifications, compound statements.

### Code Actions

The code actions system provides lightbulb-style suggestions, fixes, and
refactorings.

- **`CodeIssue`**: A diagnostic-like issue at a span. Can have `Kind` =
  `Suggestion`, `Warning`, or `Error`.
- **`CodeAction`**: An actionable fix or refactoring with `Description`,
  `Flags`, and an `Invoke()` method.
- **`ICodeIssueProvider`**: Produces code issues for a semantic model.
- **`ICodeFixProvider`**: Produces fixes for a code issue.
- **`ICodeRefactoringProvider`**: Produces refactorings for a position.

Built-in code issues:

- `UnusedVariableCodeIssueProvider` — Warns about unused variables
- `UnusedColumnCodeIssueProvider` — Warns about unused columns in the SELECT
  list
- `UnusedTableCodeIssueProvider` — Warns about unused table references

Built-in code fixes:

- `RemoveUnusedVariableCodeFixProvider`
- `RemoveUnusedColumnCodeFixProvider`
- `RemoveUnusedTableCodeFixProvider`

Built-in code refactorings:

- `IntroduceVariableCodeRefactoringProvider` — Extracts an expression into a
  variable

### Selection

The selection system provides extend/shrink selection (Ctrl+W / Ctrl+Shift+W).

- **`ISelectionSpanProvider`**: Produces selection spans for a given position.
- **`SelectionExtensions`**: Provides `ExtendSelection()`, `ShrinkSelection()`
  on `SyntaxTree`.

### Commenting

The commenting system provides toggle single-line (`--`) and toggle multi-line
(`/* */`) comment commands.

- **`SyntaxTree.ToggleSingleLineComment(span)`**: Toggles `--` on each line in
  the span.
- **`SyntaxTree.ToggleMultiLineComment(span)`**: Toggles `/* */` wrapping.
- **`SyntaxTree.ToggleComment(span)`**: Auto-detects which mode to use.

### Symbol Markup

`SymbolMarkup` is a formatted text representation of a symbol, used in quick
info and other tooltip-style UI. It supports runs of text with classification
type information, enabling rich rendering in the editor.

### Glyphs

The `Glyph` enum provides icon identifiers for symbol types (Table, Column,
Function, Aggregate, Method, Property, Variable, Keyword, etc.).
`NQueryGlyphImageSource` in the Wpf project maps these to `ImageSource` objects.

---

## Composition

The `NQuery.Authoring.Composition` project provides MEF-based service
aggregation. For each feature, it defines a service interface and an
implementation that collects all MEF-imported providers:

| Interface                            | Aggregates                              |
| ------------------------------------ | --------------------------------------- |
| `IBraceMatcherService`               | `IBraceMatcher` instances               |
| `ICompletionProviderService`         | `ICompletionProvider` instances         |
| `ICodeFixProviderService`            | `ICodeFixProvider` instances            |
| `ICodeIssueProviderService`          | `ICodeIssueProvider` instances          |
| `ICodeRefactoringProviderService`    | `ICodeRefactoringProvider` instances    |
| `IHighlighterService`                | `IHighlighter` instances                |
| `IQuickInfoModelProviderService`     | `IQuickInfoModelProvider` instances     |
| `ISignatureHelpModelProviderService` | `ISignatureHelpModelProvider` instances |
| `ISelectionSpanProviderService`      | `ISelectionSpanProvider` instances      |
| `IOutliningService`                  | `IOutliner` instances                   |

Each service concatenates MEF-imported providers with the standard built-in ones
and exposes them as an `ImmutableArray`.

---

## Shared WPF Components

The `NQuery.Authoring.Wpf` project provides editor-agnostic UI widgets:

- **`CodeActionModel`** / `CodeActionGlyphPopup` — Lightbulb glyph UI with
  context menu. States: Icon, Hovering, Expanded. Used by both VS and Actipro
  integrations.
- **`DiagnosticGrid`** / `DiagnosticsViewModel` — WPF grid for listing
  diagnostics.
  embedded PNG resources.
- **`NQueryGlyphImageSource`** — Maps `Glyph` enum values to `ImageSource` from
- **ShowPlanView** — WPF visualization of query execution plans.
- **SyntaxTreeVisualizer** — WPF `TreeView`-based syntax tree explorer.
