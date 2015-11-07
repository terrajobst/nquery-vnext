using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class SelectClauseSyntax : SyntaxNode
    {
        internal SelectClauseSyntax(SyntaxTree syntaxTree, SyntaxToken selectKeyword, SyntaxToken distinctAllKeyword, TopClauseSyntax topClause, SeparatedSyntaxList<SelectColumnSyntax> selectColumns)
            : base(syntaxTree)
        {
            SelectKeyword = selectKeyword;
            DistinctAllKeyword = distinctAllKeyword;
            TopClause = topClause;
            Columns = selectColumns;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SelectClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return SelectKeyword;
            if (DistinctAllKeyword != null)
                yield return DistinctAllKeyword;
            if (TopClause != null)
                yield return TopClause;
            foreach (var nodeOrToken in Columns.GetWithSeparators())
                yield return nodeOrToken;
        }

        public SyntaxToken SelectKeyword { get; }

        public SyntaxToken DistinctAllKeyword { get; }

        public TopClauseSyntax TopClause { get; }

        public SeparatedSyntaxList<SelectColumnSyntax> Columns { get; }
    }
}