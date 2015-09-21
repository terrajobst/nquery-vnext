using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class SoundsLikeExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _notKeyword;
        private readonly SyntaxToken _soundsKeyword;
        private readonly SyntaxToken _likeKeyword;
        private readonly ExpressionSyntax _right;

        public SoundsLikeExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken soundsKeyword, SyntaxToken likeKeyword, ExpressionSyntax right)
            : base(syntaxTree)
        {
            _left = left;
            _notKeyword = notKeyword;
            _soundsKeyword = soundsKeyword;
            _likeKeyword = likeKeyword;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SoundsLikeExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword;
            yield return _soundsKeyword;
            yield return _likeKeyword;
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

        public SyntaxToken SoundsKeyword
        {
            get { return _soundsKeyword; }
        }

        public SyntaxToken LikeKeyword
        {
            get { return _likeKeyword; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }
}