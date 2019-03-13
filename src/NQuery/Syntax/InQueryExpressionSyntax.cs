#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class InQueryExpressionSyntax : ExpressionSyntax
    {
        internal InQueryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken? notKeyword, SyntaxToken inKeyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            Expression = expression;
            NotKeyword = notKeyword;
            InKeyword = inKeyword;
            LeftParenthesis = leftParenthesis;
            Query = query;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InQueryExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Expression;
            if (NotKeyword != null)
                yield return NotKeyword;
            yield return InKeyword;
            yield return LeftParenthesis;
            yield return Query;
            yield return RightParenthesis;
        }

        public ExpressionSyntax Expression { get; }

        public SyntaxToken? NotKeyword { get; }

        public SyntaxToken InKeyword { get; }

        public SyntaxToken LeftParenthesis { get; }

        public QuerySyntax Query { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}