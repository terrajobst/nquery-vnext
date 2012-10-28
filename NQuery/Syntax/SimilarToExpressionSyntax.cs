using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class SimilarToExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _notKeyword;
        private readonly SyntaxToken _similarKeyword;
        private readonly SyntaxToken _toKeyword;
        private readonly ExpressionSyntax _right;

        public SimilarToExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken similarKeyword, SyntaxToken toKeyword, ExpressionSyntax right)
            : base(syntaxTree)
        {
            _left = left;
            _notKeyword = notKeyword;
            _similarKeyword = similarKeyword;
            _toKeyword = toKeyword;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SimilarToExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword;
            yield return _similarKeyword;
            yield return _toKeyword;
            yield return _right;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken SimilarKeyword
        {
            get { return _similarKeyword; }
        }

        public SyntaxToken ToKeyword
        {
            get { return _toKeyword; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }
}