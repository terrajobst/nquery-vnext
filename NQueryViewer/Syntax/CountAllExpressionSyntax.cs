using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class CountAllExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;
        private readonly SyntaxToken _leftParentheses;
        private readonly SyntaxToken _asteriskToken;
        private readonly SyntaxToken _rightParentheses;

        public CountAllExpressionSyntax(SyntaxToken identifier, SyntaxToken leftParentheses, SyntaxToken asteriskToken, SyntaxToken rightParentheses)
        {
            _identifier = identifier;
            _leftParentheses = leftParentheses;
            _asteriskToken = asteriskToken;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CountAllExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifier;
            yield return _leftParentheses;
            yield return _asteriskToken;
            yield return _rightParentheses;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public SyntaxToken AsteriskToken
        {
            get { return _asteriskToken; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }
}