using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class BetweenCodeRefactoringProvider : CodeRefactoringProvider<BinaryExpressionSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, BinaryExpressionSyntax node)
        {
            var isLogicalAnd = node.Kind == SyntaxKind.LogicalAndExpression;
            if (!isLogicalAnd)
                return Enumerable.Empty<ICodeAction>();

            var leftComparison = node.Left as BinaryExpressionSyntax;
            if (leftComparison == null)
                return Enumerable.Empty<ICodeAction>();

            var rightComparison = node.Right as BinaryExpressionSyntax;
            if (rightComparison == null)
                return Enumerable.Empty<ICodeAction>();

            // Let's bring the expression into the following form:
            //
            // lowerBound <= expression1 AND expression2 <= upperBound

            ExpressionSyntax lowerBound;
            ExpressionSyntax expression1;
            ExpressionSyntax expression2;
            ExpressionSyntax upperBound;

            if (!TryGetLessThanBounds(leftComparison, out lowerBound, out expression1) ||
                !TryGetLessThanBounds(rightComparison, out expression2, out upperBound))
            {
                return Enumerable.Empty<ICodeAction>();
            }

            // If expression1 and expression2 are syntactically equivalent, we can replace them with BETWEEN:
            //
            //      lowerBound <= expression1 AND expression2 <= upperBound
            //
            // would become:
            //
            //      expression1 BETWEEN lowerBound AND upperBound
            //
            // However, the logical AND may have swapped arguments:
            //
            //      expression2 <= upperBound AND lowerBound <= expression1

            if (expression1.IsEquivalentTo(expression2))
            {
                // Nothing to do, already in correct order
            }
            else if (upperBound.IsEquivalentTo(lowerBound))
            {
                // OK, we need to swap things around.
                var oldUpperBound = upperBound;
                lowerBound = expression2;
                upperBound = expression1;
                expression1 = oldUpperBound;
            }
            else
            {
                return Enumerable.Empty<ICodeAction>();
            }

            return new[] {new BetweenCodeAction(node, expression1, lowerBound, upperBound) };
        }

        private static bool TryGetLessThanBounds(BinaryExpressionSyntax expression, out ExpressionSyntax lowerBound, out ExpressionSyntax upperBound)
        {
            if (expression.Kind == SyntaxKind.LessOrEqualExpression)
            {
                lowerBound = expression.Left;
                upperBound = expression.Right;
                return true;
            }

            if (expression.Kind == SyntaxKind.GreaterOrEqualExpression)
            {
                lowerBound = expression.Right;
                upperBound = expression.Left;
                return true;
            }

            lowerBound = null;
            upperBound = null;
            return false;
        }

        private sealed class BetweenCodeAction : CodeAction
        {
            private readonly BinaryExpressionSyntax _original;
            private readonly ExpressionSyntax _expression;
            private readonly ExpressionSyntax _lowerBound;
            private readonly ExpressionSyntax _upperBound;

            public BetweenCodeAction(BinaryExpressionSyntax original, ExpressionSyntax expression, ExpressionSyntax lowerBound, ExpressionSyntax upperBound)
                : base(original.SyntaxTree)
            {
                _original = original;
                _expression = expression;
                _lowerBound = lowerBound;
                _upperBound = upperBound;
            }

            public override string Description
            {
                get { return Resources.CodeActionReplaceWithBetween; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var text = _original.SyntaxTree.Text;
                var expressionText = text.GetText(_expression.Span);
                var lowerBoundText = text.GetText(_lowerBound.Span);
                var upperBoundText = text.GetText(_upperBound.Span);
                var newText = $"{expressionText} BETWEEN {lowerBoundText} AND {upperBoundText}";
                changeSet.ReplaceText(_original.Span, newText);
            }
        }
    }
}