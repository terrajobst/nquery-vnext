#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CommonTableExpressionColumnNameListSyntax : SyntaxNode
    {
        internal CommonTableExpressionColumnNameListSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, SeparatedSyntaxList<CommonTableExpressionColumnNameSyntax> columnNames, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            LeftParenthesis = leftParenthesis;
            ColumnNames = columnNames;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnNameList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftParenthesis;
            foreach (var nodeOrToken in ColumnNames.GetWithSeparators())
                yield return nodeOrToken;
            yield return RightParenthesis;
        }

        public SyntaxToken LeftParenthesis { get; }

        public SeparatedSyntaxList<CommonTableExpressionColumnNameSyntax> ColumnNames { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}