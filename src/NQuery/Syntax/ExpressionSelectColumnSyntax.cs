using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class ExpressionSelectColumnSyntax : SelectColumnSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly AliasSyntax _alias;

        public ExpressionSelectColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, AliasSyntax alias)
            : base(syntaxTree)
        {
            _expression = expression;
            _alias = alias;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExpressionSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _expression;
            if (_alias != null)
                yield return _alias;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public AliasSyntax Alias
        {
            get { return _alias; }
        }
    }
}