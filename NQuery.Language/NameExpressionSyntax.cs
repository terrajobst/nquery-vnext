using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;

        public NameExpressionSyntax(SyntaxToken identifier)
        {
            _identifier = identifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NameExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Identifier;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }
    }
}