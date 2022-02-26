using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class NamedTableReferenceQuickInfoModelProvider : QuickInfoModelProvider<NamedTableReferenceSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, NamedTableReferenceSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
                return null;

            if (node.TableName.Span.ContainsOrTouches(position))
            {
                var span = node.TableName.Span;
                return QuickInfoModel.ForSymbol(semanticModel, span, symbol.Table);
            }

            if (node.Alias != null && node.Alias.Identifier.Span.ContainsOrTouches(position))
            {
                var span = node.Alias.Identifier.Span;
                return QuickInfoModel.ForSymbol(semanticModel, span, symbol);
            }

            return null;
        }
    }
}