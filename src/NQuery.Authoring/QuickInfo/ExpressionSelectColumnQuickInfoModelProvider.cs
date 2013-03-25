using System;
using System.ComponentModel.Composition;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class ExpressionSelectColumnQuickInfoModelProvider : QuickInfoModelProvider<ExpressionSelectColumnSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, ExpressionSelectColumnSyntax node)
        {
            if (node.Alias == null || !node.Alias.Span.Contains(position))
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Alias.Identifier.Span, symbol);
        }
    }
}
