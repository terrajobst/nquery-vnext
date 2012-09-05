using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class SoundslikeExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _soundslikeKeyword;
        private readonly ExpressionSyntax _right;

        public SoundslikeExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken? notKeyword, SyntaxToken soundslikeKeyword, ExpressionSyntax right)
            : base(syntaxTree)
        {
            _left = left;
            _notKeyword = notKeyword.WithParent(this);
            _soundslikeKeyword = soundslikeKeyword.WithParent(this);
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SoundslikeExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
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