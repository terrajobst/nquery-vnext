using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class CountAllExpressionQuickInfoModelProvider : ExpressionQuickInfoModelProvider<CountAllExpressionSyntax>
    {
        protected override bool IsMatch(SemanticModel semanticModel, int position, CountAllExpressionSyntax node)
        {
            return node.Name.Span.Contains(position);
        }
    }
}