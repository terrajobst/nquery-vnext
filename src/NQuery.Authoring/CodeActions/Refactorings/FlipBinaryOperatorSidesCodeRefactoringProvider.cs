using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class FlipBinaryOperatorSidesCodeRefactoringProvider : CodeRefactoringProvider<BinaryExpressionSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, BinaryExpressionSyntax node)
        {
            var operatorToken = node.OperatorToken;
            var canSwap = operatorToken.Span.ContainsOrTouches(position) &&
                          SyntaxFacts.CanSwapBinaryExpressionTokenKind(operatorToken.Kind);
            return canSwap
                    ? new[] {new FlipBinaryOperatorSidesCodeAction(node)}
                    : Enumerable.Empty<ICodeAction>();
        }

        private sealed class FlipBinaryOperatorSidesCodeAction : CodeAction
        {
            private readonly BinaryExpressionSyntax _node;
            private readonly string _swappedOperatorText;

            public FlipBinaryOperatorSidesCodeAction(BinaryExpressionSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;

                var operatorKind = _node.OperatorToken.Kind;
                var swappedOperatorKind = SyntaxFacts.SwapBinaryExpressionTokenKind(operatorKind);
                var operatorText = operatorKind.GetText();
                _swappedOperatorText = swappedOperatorKind.GetText();

                Description = operatorText == _swappedOperatorText
                    ? string.Format(Resources.CodeActionFlipOperands, operatorText)
                    : string.Format(Resources.CodeActionFlipOperatorTo, operatorText, _swappedOperatorText);
            }

            public override string Description { get; }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var operatorSpan = _node.OperatorToken.Span;
                var leftSpan = _node.Left.Span;
                var rightSpan = _node.Right.Span;

                var text = _node.SyntaxTree.Text;
                var left = text.GetText(leftSpan);
                var right = text.GetText(rightSpan);

                changeSet.ReplaceText(leftSpan, right);
                changeSet.ReplaceText(operatorSpan, _swappedOperatorText);
                changeSet.ReplaceText(rightSpan, left);
            }
        }
    }
}