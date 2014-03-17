using System;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo
{
    internal sealed class DerivedTableReferenceQuickInfoModelProvider : QuickInfoModelProvider<DerivedTableReferenceSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
        {
            if (!node.Name.Span.ContainsOrTouches(position))
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}