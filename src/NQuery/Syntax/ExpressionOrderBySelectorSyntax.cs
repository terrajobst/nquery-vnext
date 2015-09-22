using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class ExpressionOrderBySelectorSyntax : OrderBySelectorSyntax
    {
        private readonly ExpressionSyntax _expression;

        public ExpressionOrderBySelectorSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            _expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExpressionOrderBySelector; }
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