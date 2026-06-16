using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class NullIfQuickInfoModelProvider : QuickInfoModelProvider<NullIfExpressionSyntax>
{
    protected override QuickInfoModel? CreateModel(SemanticModel semanticModel, int position, NullIfExpressionSyntax node)
    {
        var keywordSpan = node.NullIfKeyword.Span;
        return !keywordSpan.ContainsOrTouches(position)
                   ? null
                   : new QuickInfoModel(semanticModel, keywordSpan, Glyph.Function, SymbolMarkup.ForNullIfSymbol());
    }
}
