using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CoalesceExpressionSyntax : ExpressionSyntax
    {
        internal CoalesceExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken coalesceKeyword, ArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            CoalesceKeyword = coalesceKeyword;
            ArgumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CoalesceExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return CoalesceKeyword;
            yield return ArgumentList;
        }

        public SyntaxToken CoalesceKeyword { get; }

        public ArgumentListSyntax ArgumentList { get; }
    }
}