using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class VariableExpressionSyntax : ExpressionSyntax
    {
        internal VariableExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken atToken, SyntaxToken name)
            : base(syntaxTree)
        {
            AtToken = atToken;
            Name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.VariableExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return AtToken;
            yield return Name;
        }

        public SyntaxToken AtToken { get; }

        public SyntaxToken Name { get; }
    }
}