using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CountAllExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;
        private readonly SyntaxToken _leftParenthesis;
        private readonly SyntaxToken _asteriskToken;
        private readonly SyntaxToken _rightParenthesis;

        public CountAllExpressionSyntax(SyntaxToken identifier, SyntaxToken leftParenthesis, SyntaxToken asteriskToken, SyntaxToken rightParenthesis)
        {
            _identifier = identifier;
            _leftParenthesis = leftParenthesis;
            _asteriskToken = asteriskToken;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CountAllExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifier;
            yield return _leftParenthesis;
            yield return _asteriskToken;
            yield return _rightParenthesis;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public SyntaxToken AsteriskToken
        {
            get { return _asteriskToken; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }
}