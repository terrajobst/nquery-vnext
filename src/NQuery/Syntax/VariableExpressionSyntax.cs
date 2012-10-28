using System;
using System.Collections.Generic;

namespace NQuery
{
    public sealed class VariableExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _atToken;
        private readonly SyntaxToken _name;

        public VariableExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken atToken, SyntaxToken name)
            : base(syntaxTree)
        {
            _atToken = atToken;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.VariableExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _atToken;
            yield return _name;
        }

        public SyntaxToken AtToken
        {
            get { return _atToken; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }
}