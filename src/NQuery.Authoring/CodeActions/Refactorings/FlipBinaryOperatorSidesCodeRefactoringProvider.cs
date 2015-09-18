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
            return node.OperatorToken.Span.ContainsOrTouches(position)
                    ? new[] {new FlipBinaryOperatorSidesCodeAction(node)}
                    : Enumerable.Empty<ICodeAction>();
        }

        private sealed class FlipBinaryOperatorSidesCodeAction : CodeAction
        {
            private readonly BinaryExpressionSyntax _node;

            public FlipBinaryOperatorSidesCodeAction(BinaryExpressionSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return $"Flip arguments of operator '{_node.OperatorToken.Text}'"; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var leftSpan = _node.Left.Span;
                var rightSpan = _node.Right.Span;

                var text = _node.SyntaxTree.Text;
                var left = text.GetText(leftSpan);
                var right = text.GetText(rightSpan);

                changeSet.ReplaceText(leftSpan, right);
                changeSet.ReplaceText(rightSpan, left);
            }
        }
    }
}