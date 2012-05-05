using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class SoundslikeExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _soundslikeKeyword;
        private readonly ExpressionSyntax _right;

        public SoundslikeExpressionSyntax(ExpressionSyntax left, SyntaxToken? notKeyword, SyntaxToken soundslikeKeyword, ExpressionSyntax right)
        {
            _left = left;
            _notKeyword = notKeyword;
            _soundslikeKeyword = soundslikeKeyword;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LikeExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _soundslikeKeyword;
            yield return _right;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken? NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken SoundslikeKeyword
        {
            get { return _soundslikeKeyword; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }
}