using System.ComponentModel.Composition;

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
}