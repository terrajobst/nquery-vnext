using NQuery.Authoring.Classifications;
using NQuery.Authoring.Completion;
using NQuery.Authoring.Highlighting;
using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.Authoring.Outlining;
using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.Selection;
using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SymbolSearch;
using NQuery.CodeAnalysis.Text;

using StreamJsonRpc;

using LspCompletionItem = NQuery.Authoring.LanguageServer.Protocol.CompletionItem;
using LspRange = NQuery.Authoring.LanguageServer.Protocol.Range;
using LspSignatureHelp = NQuery.Authoring.LanguageServer.Protocol.SignatureHelp;

namespace NQuery.Authoring.LanguageServer.Server;

// Every handler here warms the document asynchronously and then calls the language services
// synchronously. The services are synchronous by design -- nothing below a document does I/O -- so
// the await is purely about not parsing and binding on the RPC dispatch thread.
internal sealed partial class LanguageServerTarget
{
    [JsonRpcMethod(Methods.TextDocumentCompletion, UseSingleObjectParameterDeserialization = true)]
    public async Task<CompletionList?> CompletionAsync(CompletionParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var document = await TryGetBoundDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        if (document is null)
            return null;

        var text = document.Text;
        var view = DocumentView.Create(document, text.ToOffset(parameters.Position));
        var model = document.Services.GetService<CompletionService>().GetModel(view, cancellationToken);

        // The applicable span comes from the model rather than the client's word-boundary guess,
        // which is what makes bracketed identifiers ([Order Details]) replace correctly.
        var range = text.ToRange(model.ApplicableSpan);

        var items = model.Items.Select(item => ToCompletionItem(item, range)).ToArray();
        return new CompletionList { IsIncomplete = false, Items = items };
    }

    private static LspCompletionItem ToCompletionItem(Authoring.Completion.CompletionItem item, LspRange range)
    {
        return new LspCompletionItem
        {
            Label = item.DisplayText,
            Kind = item.Glyph?.ToCompletionItemKind(),
            Detail = item.Description,
            Preselect = item.IsBuilder ? true : null,
            FilterText = item.DisplayText,
            SortText = item.DisplayText,
            TextEdit = new TextEdit { Range = range, NewText = item.InsertionText }
        };
    }

    [JsonRpcMethod(Methods.TextDocumentHover, UseSingleObjectParameterDeserialization = true)]
    public async Task<Hover?> HoverAsync(HoverParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var document = await TryGetBoundDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        if (document is null)
            return null;

        var text = document.Text;
        var view = DocumentView.Create(document, text.ToOffset(parameters.Position));
        var model = document.Services.GetService<QuickInfoService>().GetModel(view, cancellationToken);
        if (model is null)
            return null;

        return new Hover
        {
            Contents = model.Markup.ToMarkupContent(),
            Range = text.ToRange(model.Span)
        };
    }

    [JsonRpcMethod(Methods.TextDocumentSignatureHelp, UseSingleObjectParameterDeserialization = true)]
    public async Task<LspSignatureHelp?> SignatureHelpAsync(SignatureHelpParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var document = await TryGetBoundDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        if (document is null)
            return null;

        var view = DocumentView.Create(document, document.Text.ToOffset(parameters.Position));
        var model = document.Services.GetService<SignatureHelpService>().GetModel(view, cancellationToken);
        if (model is null)
            return null;

        var signatures = model.Signatures.Select(ToSignatureInformation).ToArray();
        var activeSignature = model.Signatures.IndexOf(model.Signature);

        return new LspSignatureHelp
        {
            Signatures = signatures,
            ActiveSignature = activeSignature < 0 ? 0 : activeSignature,
            ActiveParameter = model.SelectedParameter
        };
    }

    private static SignatureInformation ToSignatureInformation(SignatureItem signature)
    {
        // ParameterItem.Span indexes into the signature's own text, which is exactly LSP's
        // offset-pair parameter label -- no substring search, so duplicate parameter names and
        // nested parentheses can't confuse the highlight.
        var parameters = signature.Parameters
                                  .Select(p => new ParameterInformation { Label = [p.Span.Start, p.Span.End] })
                                  .ToArray();

        return new SignatureInformation
        {
            Label = signature.Content,
            Parameters = parameters
        };
    }

    [JsonRpcMethod(Methods.TextDocumentDefinition, UseSingleObjectParameterDeserialization = true)]
    public async Task<Location[]?> DefinitionAsync(DefinitionParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        await document.GetSemanticModelAsync(cancellationToken);

        var text = document.Text;
        var view = DocumentView.Create(document, text.ToOffset(parameters.Position));
        var symbolSearch = document.Services.GetService<SymbolSearchService>();

        var symbolSpan = symbolSearch.FindSymbol(view, cancellationToken);
        if (symbolSpan is null)
            return null;

        // Schema tables, built-in functions and the like have no declaration in the document, so
        // this legitimately comes back empty for them; only in-query bindings (CTEs, aliases,
        // derived tables) have somewhere to go.
        return symbolSearch.FindUsages(document, symbolSpan.Value.Symbol, cancellationToken)
                           .Where(s => s.Kind == SymbolSpanKind.Definition)
                           .Select(s => new Location { Uri = snapshot.Value.Uri, Range = text.ToRange(s.Span) })
                           .ToArray();
    }

    [JsonRpcMethod(Methods.TextDocumentReferences, UseSingleObjectParameterDeserialization = true)]
    public async Task<Location[]?> ReferencesAsync(ReferenceParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        await document.GetSemanticModelAsync(cancellationToken);

        var text = document.Text;
        var view = DocumentView.Create(document, text.ToOffset(parameters.Position));
        var symbolSearch = document.Services.GetService<SymbolSearchService>();

        var symbolSpan = symbolSearch.FindSymbol(view, cancellationToken);
        if (symbolSpan is null)
            return null;

        var includeDeclaration = parameters.Context?.IncludeDeclaration ?? true;

        return symbolSearch.FindUsages(document, symbolSpan.Value.Symbol, cancellationToken)
                           .Where(s => includeDeclaration || s.Kind != SymbolSpanKind.Definition)
                           .Select(s => new Location { Uri = snapshot.Value.Uri, Range = text.ToRange(s.Span) })
                           .ToArray();
    }

    [JsonRpcMethod(Methods.TextDocumentDocumentHighlight, UseSingleObjectParameterDeserialization = true)]
    public async Task<DocumentHighlight[]?> DocumentHighlightAsync(DocumentHighlightParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var document = await TryGetBoundDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        if (document is null)
            return null;

        var text = document.Text;
        var view = DocumentView.Create(document, text.ToOffset(parameters.Position));

        // Covers both symbol references and the keyword highlighters (SELECT/FROM/WHERE,
        // CASE/END, join keywords), which is why it is not just FindUsages.
        return document.Services.GetService<HighlightingService>()
                       .GetHighlights(view, cancellationToken)
                       .Select(span => new DocumentHighlight { Range = text.ToRange(span), Kind = DocumentHighlightKind.Text })
                       .ToArray();
    }

    [JsonRpcMethod(Methods.TextDocumentSemanticTokensFull, UseSingleObjectParameterDeserialization = true)]
    public async Task<SemanticTokens?> SemanticTokensAsync(SemanticTokensParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var document = await TryGetBoundDocumentAsync(parameters.TextDocument.Uri, cancellationToken);
        if (document is null)
            return null;

        var classification = document.Services.GetService<ClassificationService>();
        var syntaxSpans = classification.ClassifySyntax(document, cancellationToken);
        var semanticSpans = classification.ClassifySemantics(document, cancellationToken);

        return new SemanticTokens { Data = ClassificationMapping.Encode(document.Text, syntaxSpans, semanticSpans) };
    }

    [JsonRpcMethod(Methods.TextDocumentFoldingRange, UseSingleObjectParameterDeserialization = true)]
    public async Task<FoldingRange[]?> FoldingRangeAsync(FoldingRangeParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        await document.GetSyntaxTreeAsync(cancellationToken);
        var text = document.Text;

        var regions = document.Services.GetService<OutliningService>().FindRegions(document, cancellationToken);
        var result = new List<FoldingRange>(regions.Length);

        foreach (var region in regions)
        {
            var range = text.ToRange(region.Span);

            // A region that does not actually span lines has nothing to collapse.
            if (range.Start.Line == range.End.Line)
                continue;

            result.Add(new FoldingRange
            {
                StartLine = range.Start.Line,
                StartCharacter = range.Start.Character,
                EndLine = range.End.Line,
                EndCharacter = range.End.Character,
                CollapsedText = region.Text
            });
        }

        return result.ToArray();
    }

    [JsonRpcMethod(Methods.TextDocumentSelectionRange, UseSingleObjectParameterDeserialization = true)]
    public async Task<SelectionRange[]?> SelectionRangeAsync(SelectionRangeParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return null;

        var document = snapshot.Value.Document;
        await document.GetSyntaxTreeAsync(cancellationToken);
        var text = document.Text;

        return parameters.Positions
                         .Select(p => BuildSelectionRange(document, text.ToOffset(p), cancellationToken))
                         .ToArray();
    }

    // ExtendSelection only widens one step at a time, so the LSP parent chain is built by walking
    // it to a fixed point. The chain is assembled outermost-first and then nested inward, because
    // SelectionRange links child -> parent.
    private static SelectionRange BuildSelectionRange(Document document, int position, CancellationToken cancellationToken)
    {
        var text = document.Text;
        var selection = document.Services.GetService<SelectionService>();

        var spans = new List<TextSpan>();
        var current = new TextSpan(position, 0);

        while (true)
        {
            var view = DocumentView.Create(document, position, current);
            var extended = selection.ExtendSelection(view, cancellationToken);
            if (extended == current || extended.Length <= current.Length)
                break;

            spans.Add(extended);
            current = extended;

            if (current.Length >= text.Length)
                break;
        }

        SelectionRange? parent = null;

        for (var i = spans.Count - 1; i >= 0; i--)
            parent = new SelectionRange { Range = text.ToRange(spans[i]), Parent = parent };

        return parent ?? new SelectionRange { Range = text.ToRange(new TextSpan(position, 0)) };
    }
}
