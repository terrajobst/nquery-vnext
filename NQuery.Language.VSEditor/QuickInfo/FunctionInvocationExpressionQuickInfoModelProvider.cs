using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class FunctionInvocationExpressionQuickInfoModelProvider : ExpressionQuickInfoModelProvider<FunctionInvocationExpressionSyntax>
    {
        protected override bool IsMatch(SemanticModel semanticModel, int position, FunctionInvocationExpressionSyntax node)
        {
            return node.Name.Span.Contains(position);
        }
    }
}