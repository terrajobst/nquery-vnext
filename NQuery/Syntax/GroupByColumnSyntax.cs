using System;
using System.Collections.Generic;

namespace NQuery
{
    // TODO: Do we need this element at all?
    public sealed class GroupByColumnSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _expression;

        public GroupByColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            _expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.GroupByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _expression;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }
    }
}