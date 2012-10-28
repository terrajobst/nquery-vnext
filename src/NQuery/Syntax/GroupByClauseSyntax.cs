using System;
using System.Collections.Generic;

namespace NQuery
{
    public sealed class GroupByClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _groupKeyword;
        private readonly SyntaxToken _byKeyword;
        private readonly SeparatedSyntaxList<GroupByColumnSyntax> _columns;

        public GroupByClauseSyntax(SyntaxTree syntaxTree, SyntaxToken groupKeyword, SyntaxToken byKeyword, SeparatedSyntaxList<GroupByColumnSyntax> columns)
            : base(syntaxTree)
        {
            _groupKeyword = groupKeyword;
            _byKeyword = byKeyword;
            _columns = columns;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.GroupByClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _groupKeyword;
            yield return _byKeyword;
            foreach (var nodeOrToken in _columns.GetWithSeparators())
                yield return nodeOrToken;
        }

        public SyntaxToken GroupKeyword
        {
            get { return _groupKeyword; }
        }

        public SyntaxToken ByKeyword
        {
            get { return _byKeyword; }
        }

        public SeparatedSyntaxList<GroupByColumnSyntax> Columns
        {
            get { return _columns; }
        }
    }
}