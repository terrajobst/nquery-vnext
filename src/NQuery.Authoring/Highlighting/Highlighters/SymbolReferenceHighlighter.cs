using NQuery.Authoring.SymbolSearch;
using NQuery.Text;

namespace NQuery.Authoring.Highlighting.Highlighters
{
    internal sealed class SymbolReferenceHighlighter : IHighlighter
    {
        public IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, int position)
        {
            ArgumentNullException.ThrowIfNull(semanticModel);

            var symbolAtPosition = semanticModel.FindSymbol(position);
            if (symbolAtPosition is null)
                return Enumerable.Empty<TextSpan>();

            return semanticModel.FindUsages(symbolAtPosition.Value.Symbol)
                                .Select(s => s.Span);
        }
    }
}