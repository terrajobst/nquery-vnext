using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class BetweenExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _notKeyword;
        private readonly SyntaxToken _betweenKeyword;
        private readonly ExpressionSyntax _lowerBound;
        private readonly SyntaxToken _andKeyword;
        private readonly ExpressionSyntax _upperBound;

        public BetweenExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken betweenKeyword, ExpressionSyntax lowerBound, SyntaxToken andKeyword, ExpressionSyntax upperBound)
            : base(syntaxTree)
        {
            _left = left;
            _notKeyword = notKeyword;
            _betweenKeyword = betweenKeyword;
            _lowerBound = lowerBound;
            _andKeyword = andKeyword;
            _upperBound = upperBound;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.BetweenExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword;
            yield return _betweenKeyword;
            yield return _lowerBound;
            yield return _andKeyword;
            yield return _upperBound;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken BetweenKeyword
        {
            get { return _betweenKeyword; }
        }

        public ExpressionSyntax LowerBound
        {
            get { return _lowerBound; }
        }

        public SyntaxToken AndKeyword
        {
            get { return _andKeyword; }
        }

        public ExpressionSyntax UpperBound
        {
            get { return _upperBound; }
        }
    }
}