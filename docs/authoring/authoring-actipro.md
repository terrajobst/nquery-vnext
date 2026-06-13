# Authoring: Actipro SyntaxEditor Integration

The `NQuery.Authoring.ActiproWpf` project integrates NQuery's authoring services
into the Actipro WPF SyntaxEditor. It registers all services programmatically
via `NQueryLanguage` (Actipro's `SyntaxLanguage` base class). Unlike the VS
Editor integration, this project does **not** use MEF — it has its own service
wiring and does not reference `NQuery.Authoring.Composition`.

---

## Core Infrastructure

### NQueryLanguage

`NQueryLanguage` extends Actipro's `SyntaxLanguage` and is the central entry
point. Its constructor registers all services:

```csharp
public sealed class NQueryLanguage : SyntaxLanguage
{
    public NQueryLanguage();
}
```

Services are registered via `RegisterService<T>()` (Actipro's `IServiceLocator`
pattern).

### NQueryParseData

`NQueryParseData` implements Actipro's `IParseData` interface, wrapping an
NQuery `SyntaxTree`:

```csharp
public sealed class NQueryParseData : IParseData
{
    public SyntaxTree SyntaxTree { get; }
}
```

### NQueryParseDataSynchronizer

`NQueryParseDataSynchronizer` bridges the NQuery `Workspace` and Actipro's
`ICodeDocument`:

- Subscribes to `Workspace.CurrentDocumentChanged`
- On each change, gets the `SyntaxTree` asynchronously and assigns it to
  `_codeDocument.ParseData = new NQueryParseData(syntaxTree)`

### NQueryExtensions

`NQueryExtensions` provides integration extension methods:

- `ICodeDocument.GetWorkspace()` — creates or retrieves a `Workspace` stored in
  document properties, wired to an `ActiproSourceTextContainer`, and creates a
  `NQueryParseDataSynchronizer`
- `SyntaxEditor.GetDocumentView()` — creates a `DocumentView` from the editor's
  active view, accounting for line break length differences (Actipro uses `\n`,
  NQuery uses `\r\n`)

---

## Text Bridge

The text bridge adapts Actipro's `ITextDocument`/`ITextSnapshot` to NQuery's types:

- **`ActiproSourceTextContainer`** — wraps `ITextDocument`, fires
  `CurrentChanged` on `TextDocument.TextChanged`
- **`ActiproSourceText`** — wraps `ITextSnapshot` as a `SourceText`
- **`ActiproTextExtensions`** — provides conversion extensions using
  `ConditionalWeakTable`, including `ToSourceTextContainer()`, `ToSourceText()`,
  `ToTextSnapshot()`, and span-aware offset conversions

---

## Services Registered

### Classification Types

`INQueryClassificationTypes` defines Actipro `IClassificationType` values for
all syntax and semantic categories: `WhiteSpace`, `Comment`, `Identifier`,
`Keyword`, `Punctuation`, `NumberLiteral`, `StringLiteral`, `Operator`,
`SchemaTable`, `DerivedTable`, `CommonTableExpression`, `Column`, `Method`,
`Property`, `Function`, `Aggregate`, `Variable`, `Unnecessary`.

### Classification

Three `CodeDocumentTaggerProvider` implementations:

| Provider                                  | Classification                                                 |
| ----------------------------------------- | -------------------------------------------------------------- |
| `NQuerySyntacticClassifierProvider`       | Syntax-based highlighting (keywords, literals, comments, etc.) |
| `NQuerySemanticClassifierProvider`        | Semantic highlighting by symbol type (tables, columns, etc.)   |
| `NQueryUnnecessaryCodeClassifierProvider` | Unnecessary/dead code highlighting                             |

### Brace Matching / Structure Matcher

`NQueryBraceMatcher` implements Actipro's `IStructureMatcher`:

- Uses `SyntaxTree.MatchBraces(position, Matchers)` with standard brace matchers
- Returns `StructureMatchResultSet` with matching spans
- Wrapped with `DelimiterHighlightTagger` for visual brace highlighting

### Completion

- **`NQueryCompletionProvider`** — extends Actipro's `CompletionProviderBase`.
  Uses `SemanticModel.GetCompletionModel(position, Providers)` to compute items.
  Creates `NQueryCompletionSession`.
- **`NQueryCompletionController`** — implements
  `IEditorDocumentTextChangeEventSink`. On text changed, triggers completion on
  trigger characters and commits on commit characters.

### Quick Info

`NQueryQuickInfoProvider` extends Actipro's `QuickInfoProviderBase`. Uses
`SemanticModel.GetQuickInfoModel(position, Providers)` and renders content via
`INQuerySymbolContentProvider`.

### Signature Help

- **`NQuerySignatureHelpProvider`** — extends `ParameterInfoProviderBase`. Uses
  `SemanticModel.GetSignatureHelpModel(position, Providers)` to compute
  signatures. Creates `ParameterInfoSession` with
  `NQuerySignatureContentProvider` items.
- **`NQuerySignatureHelpController`** — implements
  `IEditorDocumentTextChangeEventSink` and
  `IEditorViewSelectionChangeEventSink`. Triggers parameter info on `(` and `,`
  and on caret movement.

### Outlining

- **`NQueryOutliner`** — implements Actipro's `IOutliner`. Wraps standard
  outliners.
- **`NQueryOutliningSource`** — implements `IOutliningSource`, providing
  outlining regions from syntax tree node spans.

### Squiggles (Error Underlines)

Three `CodeDocumentTaggerProvider` implementations use `NQuerySquiggleClassifier` as a base:

| Provider                                        | Source                           |
| ----------------------------------------------- | -------------------------------- |
| `NQuerySyntaxErrorSquiggleClassifierProvider`   | `SyntaxTree.GetDiagnostics()`    |
| `NQuerySemanticErrorSquiggleClassifierProvider` | `SemanticModel.GetDiagnostics()` |
| `NQuerySemanticIssueSquiggleClassifierProvider` | `ICodeIssueProvider`             |

### Code Actions

- **`ExpandCodeActionListEditAction`** — `EditActionBase` that expands the code
  action lightbulb
- **`TextDocumentCodeActionModel`** — applies code action edits via
  `ITextDocument`
- **`ICodeActionGlyphController`** — shared interface for managing the glyph
  popup

Code action commands must be registered explicitly:

```csharp
editor.RegisterCodeActionCommands(syntaxEditor);
```

### Margins (Code Action Glyph)

`NQueryEditorViewCodeActionMargin` implements `IEditorViewMargin` and
`ICodeActionGlyphController`. It draws a lightbulb glyph in the `ScrollableLeft`
margin area using the shared `CodeActionGlyphPopup` from `NQuery.Authoring.Wpf`.

### Commenting

- **`ToggleSingleLineCommentAction`** — toggles `--` on each line
- **`ToggleMultiLineCommentAction`** — toggles `/* */` wrapping

Both extend `ToggleCommentAction` (`EditActionBase`), which calls
`SyntaxTree.ToggleComment(span)`. Commands must be registered explicitly:

```csharp
editor.RegisterCommentingCommands(syntaxEditor);
```

### Selection

- **`ExtendSelectionAction`** — extends selection outward (Ctrl+W)
- **`ShrinkSelectionAction`** — shrinks selection inward (Ctrl+Shift+W)

Both extend `SelectionAction` (`EditActionBase`), which uses
`SyntaxTree.ExtendSelection()` with a `SelectionHandler` that maintains a stack
of selection spans. Commands must be registered explicitly:

```csharp
editor.RegisterSelectionCommands(syntaxEditor);
```

### Symbol Content

`INQuerySymbolContentProvider` creates Actipro `IContentProvider` (rich HTML
content) and `IImageSourceProvider` from NQuery symbols:

- **`NQuerySymbolContentProvider`** — uses classification types and highlighting
  styles to create formatted HTML
- **`HtmlContentProviderWithGlyph`** — provides HTML tooltip content with a
  glyph image
- **`GlyphImageProvider`** — maps `Glyph` enum to `ImageSource`
- **`HtmlMarkupEmitter`** — generates HTML from `SymbolMarkup` runs
