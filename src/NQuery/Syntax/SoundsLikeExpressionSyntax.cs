using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class SoundsLikeExpressionSyntax : ExpressionSyntax
    {
        internal SoundsLikeExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken soundsKeyword, SyntaxToken likeKeyword, ExpressionSyntax right)
            : base(syntaxTree)
        {
            Left = left;
            NotKeyword = notKeyword;
            SoundsKeyword = soundsKeyword;
            LikeKeyword = likeKeyword;
            Right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SoundsLikeExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            if (NotKeyword != null)
                yield return NotKeyword;
            yield return SoundsKeyword;
            yield return LikeKeyword;
            yield return Right;
        }

        public ExpressionSyntax Left { get; }

        public SyntaxToken NotKeyword { get; }

        public SyntaxToken SoundsKeyword { get; }

        public SyntaxToken LikeKeyword { get; }

        public ExpressionSyntax Right { get; }
    }
}