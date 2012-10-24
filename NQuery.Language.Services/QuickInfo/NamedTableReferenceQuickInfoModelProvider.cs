using System;
using System.ComponentModel.Composition;

namespace NQuery.Language.Services.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class NamedTableReferenceQuickInfoModelProvider : QuickInfoModelProvider<NamedTableReferenceSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, NamedTableReferenceSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
                return null;

            if (node.TableName.Span.Contains(position))
            {
                SyntaxNodeOrToken nodeOrToken = node.TableName;
                return QuickInfoModel.ForSymbol(semanticModel, nodeOrToken.Span, symbol.Table);
            }

            if (node.Alias != null && node.Alias.Span.Contains(position))
            {
                SyntaxNodeOrToken nodeOrToken = node.Alias;
                return QuickInfoModel.ForSymbol(semanticModel, nodeOrToken.Span, symbol);
            }

            return null;
        }
    }
}