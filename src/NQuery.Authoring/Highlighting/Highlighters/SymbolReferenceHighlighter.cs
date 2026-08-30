using NQuery.Authoring.SymbolSearch;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting.Highlighters;

internal sealed class SymbolReferenceHighlighter : IHighlighter
{
    private readonly SymbolSearchService _symbolSearch;

    public SymbolReferenceHighlighter(SymbolSearchService symbolSearch)
    {
        _symbolSearch = symbolSearch;
    }

    public IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, int position)
    {
        ThrowIfNull(semanticModel);

        var symbolAtPosition = _symbolSearch.FindSymbol(semanticModel, position);
        if (symbolAtPosition is null)
            return [];

        return _symbolSearch.FindUsages(semanticModel, symbolAtPosition.Value.Symbol)
                            .Select(s => s.Span);
    }
}
