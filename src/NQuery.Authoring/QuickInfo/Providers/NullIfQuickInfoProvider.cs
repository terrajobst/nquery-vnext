using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class NullIfQuickInfoProvider : QuickInfoProvider<NullIfExpressionSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, NullIfExpressionSyntax node)
    {
        var keywordSpan = node.NullIfKeyword.Span;
        return !keywordSpan.ContainsOrTouches(position)
                   ? null
                   : new QuickInfoResult(semanticModel, keywordSpan, Glyph.Function, SymbolMarkup.ForNullIfSymbol());
    }
}
