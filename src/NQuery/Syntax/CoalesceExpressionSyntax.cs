using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CoalesceExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _coalesceKeyword;
        private readonly ArgumentListSyntax _argumentList;

        public CoalesceExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken coalesceKeyword, ArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            _coalesceKeyword = coalesceKeyword;
            _argumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CoalesceExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _coalesceKeyword;
            yield return _argumentList;
        }

        public SyntaxToken CoalesceKeyword
        {
            get { return _coalesceKeyword; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return _argumentList; }
        }
    }
}