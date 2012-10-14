using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class MethodInvocationExpressionQuickInfoModelProvider : ExpressionQuickInfoModelProvider<MethodInvocationExpressionSyntax>
    {
        protected override bool IsMatch(SemanticModel semanticModel, int position, MethodInvocationExpressionSyntax node)
        {
            return node.Name.Span.Contains(position);
        }
    }
}