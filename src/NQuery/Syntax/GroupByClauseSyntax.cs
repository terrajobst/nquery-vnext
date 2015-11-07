using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class GroupByClauseSyntax : SyntaxNode
    {
        internal GroupByClauseSyntax(SyntaxTree syntaxTree, SyntaxToken groupKeyword, SyntaxToken byKeyword, SeparatedSyntaxList<GroupByColumnSyntax> columns)
            : base(syntaxTree)
        {
            GroupKeyword = groupKeyword;
            ByKeyword = byKeyword;
            Columns = columns;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.GroupByClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return GroupKeyword;
            yield return ByKeyword;
            foreach (var nodeOrToken in Columns.GetWithSeparators())
                yield return nodeOrToken;
        }

        public SyntaxToken GroupKeyword { get; }

        public SyntaxToken ByKeyword { get; }

        public SeparatedSyntaxList<GroupByColumnSyntax> Columns { get; }
    }
}