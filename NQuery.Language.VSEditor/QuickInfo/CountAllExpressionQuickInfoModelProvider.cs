using System.ComponentModel.Composition;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class CountAllExpressionQuickInfoModelProvider : QuickInfoModelProvider<CountAllExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CountAllExpressionSyntax node)
        {
            if (!node.Name.Span.Contains(position))
                return null;

            var symbol = semanticModel.GetSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }

    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class CoalesceExpressionQuickInfoModelProvider : QuickInfoModelProvider<CoalesceExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CoalesceExpressionSyntax node)
        {
            var keywordSpan = node.CoalesceKeyword.Span;
            return !keywordSpan.Contains(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, NQueryGlyph.Function, SymbolMarkup.ForCoalesceSymbol());
        }
    }

    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class CastExpressionQuickInfoModelProvider : QuickInfoModelProvider<CastExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CastExpressionSyntax node)
        {
            var keywordSpan = node.CastKeyword.Span;
            return !keywordSpan.Contains(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, NQueryGlyph.Function, SymbolMarkup.ForCastSymbol());
        }
    }

    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class NullIfQuickInfoModelProvider : QuickInfoModelProvider<NullIfExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, NullIfExpressionSyntax node)
        {
            var keywordSpan = node.NullIfKeyword.Span;
            return !keywordSpan.Contains(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, NQueryGlyph.Function, SymbolMarkup.ForNullIfSymbol());
        }
    }

}