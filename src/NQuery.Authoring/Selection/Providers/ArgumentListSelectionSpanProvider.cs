using NQuery.Syntax;

namespace NQuery.Authoring.Selection.Providers
{
    internal sealed class ArgumentListSelectionSpanProvider : SeparatedSyntaxListSelectionSpanProvider<ArgumentListSyntax, ExpressionSyntax>
    {
        protected override SeparatedSyntaxList<ExpressionSyntax> GetList(ArgumentListSyntax node)
        {
            return node.Arguments;
        }
    }
}