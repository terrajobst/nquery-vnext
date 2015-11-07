using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class LikeExpressionSyntax : ExpressionSyntax
    {
        internal LikeExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken likeKeyword, ExpressionSyntax right)
            : base(syntaxTree)
        {
            Left = left;
            NotKeyword = notKeyword;
            LikeKeyword = likeKeyword;
            Right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LikeExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            if (NotKeyword != null)
                yield return NotKeyword;
            yield return LikeKeyword;
            yield return Right;
        }

        public ExpressionSyntax Left { get; }

        public SyntaxToken NotKeyword { get; }

        public SyntaxToken LikeKeyword { get; }

        public ExpressionSyntax Right { get; }
    }
}