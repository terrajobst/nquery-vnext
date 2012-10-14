using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class VariableExpressionQuickInfoModelProvider : ExpressionQuickInfoModelProvider<VariableExpressionSyntax>
    {
        protected override bool IsMatch(SemanticModel semanticModel, int position, VariableExpressionSyntax node)
        {
            return node.Name.Span.Contains(position);
        }
    }
}