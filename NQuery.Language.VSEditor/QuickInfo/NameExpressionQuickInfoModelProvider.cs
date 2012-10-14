using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor
{
    [Export(typeof (IQuickInfoModelProvider))]
    internal sealed class NameExpressionQuickInfoModelProvider : ExpressionQuickInfoModelProvider<NameExpressionSyntax>
    {
        protected override bool IsMatch(SemanticModel semanticModel, int position, NameExpressionSyntax node)
        {
            return node.Name.Span.Contains(position);
        }
    }
}