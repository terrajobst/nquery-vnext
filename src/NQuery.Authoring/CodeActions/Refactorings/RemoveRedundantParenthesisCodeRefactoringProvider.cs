using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.CodeActions.Refactorings;

internal sealed class RemoveRedundantParenthesisCodeRefactoringProvider : CodeRefactoringProvider<ParenthesizedExpressionSyntax>
{
    protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, ParenthesizedExpressionSyntax node)
    {
        var inParentheses = !node.LeftParenthesis.IsMissing && node.LeftParenthesis.Span.ContainsOrTouches(position) ||
                            !node.RightParenthesis.IsMissing && node.RightParenthesis.Span.ContainsOrTouches(position);
        if (!inParentheses)
            return [];

        if (!SyntaxFacts.ParenthesisIsRedundant(node))
            return [];

        return new[] { new RemoveRedundantParenthesisCodeAction(node) };
    }

    private sealed class RemoveRedundantParenthesisCodeAction : CodeAction
    {
        private readonly ParenthesizedExpressionSyntax _expression;

        public RemoveRedundantParenthesisCodeAction(ParenthesizedExpressionSyntax expression)
            : base(expression.SyntaxTree)
        {
            _expression = expression;
        }

        public override string Description => Resources.CodeActionRemoveRedundantParenthesis;

        protected override void GetChanges(TextChangeSet changeSet)
        {
            changeSet.DeleteText(_expression.LeftParenthesis.Span);
            changeSet.DeleteText(_expression.RightParenthesis.Span);
        }
    }
}
