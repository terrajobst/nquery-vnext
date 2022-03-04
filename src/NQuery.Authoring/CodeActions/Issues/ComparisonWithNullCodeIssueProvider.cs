using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class ComparisonWithNullCodeIssueProvider : CodeIssueProvider<BinaryExpressionSyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, BinaryExpressionSyntax node)
        {
            var leftIsNull = IsNullLiteral(node.Left);
            var rightIsNull = IsNullLiteral(node.Right);
            if (!leftIsNull && !rightIsNull)
                yield break;

            var isEquals = node.OperatorToken.Kind == SyntaxKind.EqualsToken;
            var isNotEquals = node.OperatorToken.Kind == SyntaxKind.ExclamationEqualsToken ||
                              node.OperatorToken.Kind == SyntaxKind.LessGreaterToken;
            var isComparison = isEquals || isNotEquals;

            if (!isComparison)
            {
                yield return new CodeIssue(CodeIssueKind.Warning, node.Span, Resources.CodeIssueExpressionIsAlwaysNull);
            }
            else
            {
                var action = new[] { new ConvertToIsNullCodeAction(node, isEquals) };
                yield return new CodeIssue(CodeIssueKind.Warning, node.Span, Resources.CodeIssueComparsionWithNullIsAlwaysNull, action);
            }
        }

        private static bool IsNullLiteral(ExpressionSyntax node)
        {
            var literal = node as LiteralExpressionSyntax;
            return literal is not null && literal.Token.Kind == SyntaxKind.NullKeyword;
        }

        private sealed class ConvertToIsNullCodeAction : CodeAction
        {
            private readonly BinaryExpressionSyntax _node;
            private readonly bool _isEquals;

            public ConvertToIsNullCodeAction(BinaryExpressionSyntax node, bool isEquals)
                : base(node.SyntaxTree)
            {
                _node = node;
                _isEquals = isEquals;
            }

            public override string Description
            {
                get { return _isEquals ? Resources.CodeActionConvertToIsNull : Resources.CodeActionConvertToIsNotNull; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var newText = _isEquals ? @" IS NULL" : @" IS NOT NULL";

                var useLeft = IsNullLiteral(_node.Right);
                if (useLeft)
                {
                    var replacementSpan = TextSpan.FromBounds(_node.Left.Span.End, _node.Span.End);
                    changeSet.ReplaceText(replacementSpan, newText);
                }
                else
                {
                    var removalSpan = TextSpan.FromBounds(_node.Span.Start, _node.Right.Span.Start);
                    changeSet.DeleteText(removalSpan);
                    changeSet.InsertText(_node.Right.Span.End, newText);
                }
            }
        }
    }
}