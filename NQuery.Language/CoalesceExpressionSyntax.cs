using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CoalesceExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _coalesceKeyword;
        private readonly ArgumentListSyntax _arguments;

        public CoalesceExpressionSyntax(SyntaxToken coalesceKeyword, ArgumentListSyntax arguments)
        {
            _coalesceKeyword = coalesceKeyword;
            _arguments = arguments;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CoalesceExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _coalesceKeyword;
            yield return _arguments;
        }

        public SyntaxToken CoalesceKeyword
        {
            get { return _coalesceKeyword; }
        }

        public ArgumentListSyntax Arguments
        {
            get { return _arguments; }
        }
    }
}