using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class WildcardSelectColumnQuickInfoModelProvider : QuickInfoModelProvider<WildcardSelectColumnSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, WildcardSelectColumnSyntax node)
        {
            var tableName = node.TableName;
            if (tableName == null || !tableName.Span.ContainsOrTouches(position))
                return null;

            var tableInstanceSymbol = semanticModel.GetTableInstance(node);
            return tableInstanceSymbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, tableName.Span, tableInstanceSymbol);
        }
    }
}