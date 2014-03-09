using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

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

        private sealed class FlipBinaryOperatorSidesCodeAction : ICodeAction
        {
            private readonly BinaryExpressionSyntax _node;

            public FlipBinaryOperatorSidesCodeAction(BinaryExpressionSyntax node)
            {
                _node = node;
            }

            public string Description
            {
                get { return string.Format("Flip '{0}' operator arguments", _node.OperatorToken.Text); }
            }

            public SyntaxTree GetEdit()
            {
                var leftSpan = _node.Left.Span;
                var rightSpan = _node.Right.Span;
                var secondLength = rightSpan.Start - leftSpan.End;

                var textBuffer = _node.SyntaxTree.TextBuffer;
                var left = textBuffer.GetText(leftSpan);
                var right = textBuffer.GetText(rightSpan);
                
                var first = textBuffer.GetText(0, leftSpan.Start);
                var second = textBuffer.GetText(leftSpan.End, secondLength);
                var third = textBuffer.GetText(rightSpan.End);

                var newSource = first + right + second + left + third;
                return SyntaxTree.ParseQuery(newSource);
            }
        }
    }
}