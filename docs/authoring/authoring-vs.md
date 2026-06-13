# Authoring: Visual Studio Editor Integration

The `NQuery.Authoring.VSEditorWpf` project integrates NQuery's authoring
services into the Visual Studio Editor (also usable standalone via
`Microsoft.VisualStudio.VSEditor.Standalone`). It uses MEF for all service
imports and exports.

---

## Content Type Registration

The integration registers an `"NQuery"` content type with base `"Code"` via a
MEF export. This content type activates all taggers and providers when editing
`.nquery` files or any buffer with the `NQuery` content type.

```csharp
[Export(typeof(ContentTypeDefinition))]
[Name("NQuery")]
[BaseDefinition("Code")]
internal static ContentTypeDefinition NQueryContentType = null;
```

---

## Text Bridge

The text bridge adapts VS Editor's `ITextBuffer`/`ITextSnapshot` to NQuery's
`SourceTextContainer`/`SourceText`:

- **`VisualStudioSourceTextContainer`** — wraps `ITextBuffer`, fires
  `CurrentChanged` on text buffer changes
- **`VisualStudioSourceText`** — wraps `ITextSnapshot` as a `SourceText`
- **`VisualStudioTextExtensions`** — provides conversion extensions
  (`ToSourceText()`, `ToTextSnapshot()`, `ToSourceTextContainer()`,
  `ToTextBuffer()`) using `ConditionalWeakTable`

---

## Workspace and Document

**`NQueryExtensions.cs`** provides extension methods:

- `ITextBuffer.GetWorkspace()` — creates or retrieves a `Workspace` stored in
  `textBuffer.Properties`, wired to a `VisualStudioSourceTextContainer`
- `ITextBuffer.GetDocument()` — gets the current `Document` from the workspace
- `ITextView.GetDocumentView()` — creates a `DocumentView` from the text view's
  caret and selection

---

## Services

### Classification

Three classification taggers:

| Tagger                            | Tags                                                     | Description                         |
| --------------------------------- | -------------------------------------------------------- | ----------------------------------- |
| `NQuerySyntaxClassifier`          | Syntax highlighting (keywords, strings, comments, etc.)  | Tags tokens by their `SyntaxKind`   |
| `NQuerySemanticClassifier`        | Semantic highlighting (tables, columns, functions, etc.) | Tags resolved symbols by their type |
| `NQueryUnnecessaryCodeClassifier` | Faded/dead code                                          | Tags unused references              |

All taggers extend a shared `AsyncTagger<TTag, TRawTag>` base class that
computes tags on a background thread, stores them in `ImmutableArray`, and fires
`TagsChanged` when ready.

Classification types are registered via `INQueryClassificationService`, mapping
NQuery classification names to VS `IClassificationType` instances.

### Brace Matching

braces. Uses `IBraceMatcherService` from the Composition project and
`NQueryBraceTagger` (via `NQueryBraceTaggerProvider`) highlights matching
`SyntaxTree.MatchBraces()`.

### Completion

Completion is managed by `CompletionModelManager` (per-text-view), which:

- Listens to `Workspace.CurrentDocumentChanged` to recompute the completion model
- Creates/dismisses `ICompletionSession` via `ICompletionBroker`
- Uses `SemanticModel.GetCompletionModel(position, providers)` to compute items
- Bridges NQuery `CompletionItem` values to VS `CompletionSet` entries with
  glyphs via `NQueryGlyphImageSource`

`NQueryKeyProcessor` auto-triggers completion on letter/underscore/dot input.

### Quick Info

`QuickInfoManager` creates/dismisses `IQuickInfoSession` via `IQuickInfoBroker`.
Uses `SemanticModel.GetQuickInfoModel(position, providers)` to compute content.
Triggered on mouse hover via `NQueryQuickInfoTrigger`.

### Signature Help

`SignatureHelpManager` manages `ISignatureHelpSession` via
`ISignatureHelpBroker`. Uses `SemanticModel.GetSignatureHelpModel(position,
providers)`. Implements `NQueryParameter` and `NQuerySignature` as VS editor
`IParameter`/`ISignature` adapters.

### Highlighting (Reference Highlighting)

`NQueryHighlightingTagger` (via `NQueryHighlightingTaggerProvider`) highlights
all references to the symbol at the caret position.
`HighlightingNavigationManager` provides Ctrl+Shift+Up/Down navigation between
highlights.

### Squiggles (Error Underlines)

Three squiggle taggers provide error underlines:

| Tagger | Source |
|--------|--------|
| `NQuerySyntaxErrorTagger` | `SyntaxTree.GetDiagnostics()` |
| `NQuerySemanticErrorTagger` | `SemanticModel.GetDiagnostics()` |
| `NQueryCodeIssueTagger` | `ICodeIssueProviderService` |

All produce `IErrorTag` and use `IViewTaggerProvider`.

### Outlining

`NQueryOutliningTagger` creates collapsible regions from syntax tree node spans,
producing `IOutliningRegionTag`.

### Code Actions

Code actions (lightbulb) use:

- **`CodeActionGlyphBroker`** — MEF export that retrieves the
  `ICodeActionGlyphController` per text view
- **`NQueryCodeActionsMargin`** — `IWpfTextViewMargin` that draws the lightbulb
  glyph and shows a `CodeActionGlyphPopup` (shared from `NQuery.Authoring.Wpf`)
- **`TextBufferCodeActionModel`** — applies code action edits via `ITextBuffer`
  and `ITextBufferUndoManager`
- **`NQueryKeyProcessor`** — handles Ctrl+. to expand the code action list

### Commenting

`CommentOperations` (via `ICommentOperationsProvider`) toggles single-line
(`--`) and multi-line (`/* */`) comments using
`SyntaxTree.ToggleSingleLineComment()` and `ToggleMultiLineComment()`. Supports
undo via `ITextBufferUndoManager`.

### Selection

`NQuerySelectionProvider` provides extend/shrink selection. Uses
`ISelectionSpanProviderService` from Composition and
`SyntaxTree.ExtendSelection()`.

---

## Key Processing

The single `NQueryKeyProcessor` handles all keyboard interactions:

| Key                     | Action                          |
| ----------------------- | ------------------------------- |
| Ctrl+Space / Ctrl+J     | Trigger completion              |
| Ctrl+Shift+Space        | Trigger signature help          |
| Ctrl+Shift+Up/Down      | Navigate between highlights     |
| Ctrl+Alt+/              | Toggle single-line comment      |
| Ctrl+Shift+/            | Toggle multi-line comment       |
| Ctrl+.                  | Expand code action list         |
| Tab/Enter               | Commit selected completion item |
| Escape                  | Dismiss intellisense            |
| Letter, underscore, dot | Auto-trigger completion         |

---

## MEF Architecture

All integration points use the standard VS Editor MEF pattern:

- **`IViewTaggerProvider`** / **`ITaggerProvider`** for classification, brace
  matching, highlighting, squiggles, outlining
- **`IKeyProcessorProvider`** for keyboard handling
- **`IWpfTextViewMarginProvider`** for the code actions margin
- **`ICompletionSourceProvider`** for completion (delegates to
  `CompletionModelManager`)
- **`IQuickInfoSourceProvider`** for quick info
- **`ISignatureHelpSourceProvider`** for signature help

Services from `NQuery.Authoring.Composition` are imported via `[Import]` and
used within tagger/provider constructors.
