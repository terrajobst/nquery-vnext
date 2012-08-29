using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _name;

        public NameExpressionSyntax(SyntaxToken name)
        {
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NameExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Name;
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }
}