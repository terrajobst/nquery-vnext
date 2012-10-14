using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class PropertyAccessExpressionQuickInfoModelProvider : ExpressionQuickInfoModelProvider<PropertyAccessExpressionSyntax>
    {
        protected override bool IsMatch(SemanticModel semanticModel, int position, PropertyAccessExpressionSyntax node)
        {
            return node.Name.Span.Contains(position);
        }
    }
}