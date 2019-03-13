#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CommonTableExpressionSyntax : SyntaxNode
    {
        internal CommonTableExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken? recursiveKeyword, SyntaxToken name, CommonTableExpressionColumnNameListSyntax? columnNameList, SyntaxToken asKeyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            RecursiveKeyword = recursiveKeyword;
            Name = name;
            ColumnNameList = columnNameList;
            AsKeyword = asKeyword;
            LeftParenthesis = leftParenthesis;
            Query = query;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            if (RecursiveKeyword != null)
                yield return RecursiveKeyword;
            yield return Name;
            if (ColumnNameList != null)
                yield return ColumnNameList;
            yield return AsKeyword;
            yield return LeftParenthesis;
            yield return Query;
            yield return RightParenthesis;
        }

        public SyntaxToken? RecursiveKeyword { get; }

        public SyntaxToken Name { get; }

        public CommonTableExpressionColumnNameListSyntax? ColumnNameList { get; }

        public SyntaxToken AsKeyword { get; }

        public SyntaxToken LeftParenthesis { get; }

        public QuerySyntax Query { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}