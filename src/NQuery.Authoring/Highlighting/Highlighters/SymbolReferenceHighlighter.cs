using NQuery.Authoring.SymbolSearch;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting.Highlighters;

internal sealed class SymbolReferenceHighlighter : IHighlighter
{
    private readonly SymbolSearchService _symbolSearch;

    public SymbolReferenceHighlighter(SymbolSearchService symbolSearch)
    {
        _symbolSearch = symbolSearch;
    }

    public IEnumerable<TextSpan> GetHighlights(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var symbolAtPosition = _symbolSearch.FindSymbol(view, cancellationToken);
        if (symbolAtPosition is null)
            return [];

        return _symbolSearch.FindUsages(view.Document, symbolAtPosition.Value.Symbol, cancellationToken)
                            .Select(s => s.Span);
    }
}
