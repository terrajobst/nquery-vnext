using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ParameterExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _atToken;
        private readonly SyntaxToken _name;

        public ParameterExpressionSyntax(SyntaxToken atToken, SyntaxToken name)
        {
            _atToken = atToken;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParameterExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
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