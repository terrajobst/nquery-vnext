using System;
using System.ComponentModel.Composition;

using System.Linq;

namespace NQuery.Authoring.QuickInfo
{
    [Export(typeof (IQuickInfoModelProvider))]
    internal sealed class WildcardSelectColumnQuickInfoModelProvider : QuickInfoModelProvider<WildcardSelectColumnSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, WildcardSelectColumnSyntax node)
        {
            var tableName = node.TableName;
            if (tableName == null || !tableName.Span.Contains(position))
                return null;
            
            var tableInstanceSymbol = semanticModel.GetTableInstance(node);
            return tableInstanceSymbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, tableName.Span, tableInstanceSymbol);
        }
    }
}