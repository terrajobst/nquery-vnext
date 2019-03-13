#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class InExpressionSyntax : ExpressionSyntax
    {
        internal InExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken? notKeyword, SyntaxToken inKeyword, ArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            Expression = expression;
            NotKeyword = notKeyword;
            InKeyword = inKeyword;
            ArgumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Expression;
            if (NotKeyword != null)
                yield return NotKeyword;
            yield return InKeyword;
            yield return ArgumentList;
        }

        public ExpressionSyntax Expression { get; }

        public SyntaxToken? NotKeyword { get; }

        public SyntaxToken InKeyword { get; }

        public ArgumentListSyntax ArgumentList { get; }
    }
}