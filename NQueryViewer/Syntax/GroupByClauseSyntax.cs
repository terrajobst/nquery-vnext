using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQueryViewer.Syntax
{
    public sealed class GroupByClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _groupKeyword;
        private readonly SyntaxToken _byKeyword;
        private readonly ReadOnlyCollection<GroupByColumnSyntax> _columns;

        public GroupByClauseSyntax(SyntaxToken groupKeyword, SyntaxToken byKeyword, IList<GroupByColumnSyntax> columns)
        {
            _groupKeyword = groupKeyword;
            _byKeyword = byKeyword;
            _columns = new ReadOnlyCollection<GroupByColumnSyntax>(columns);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.GroupByClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _groupKeyword;
            yield return _byKeyword;
            foreach (var columnSyntax in _columns)
                yield return columnSyntax;
        }

        public SyntaxToken GroupKeyword
        {
            get { return _groupKeyword; }
        }

        public SyntaxToken ByKeyword
        {
            get { return _byKeyword; }
        }

        public ReadOnlyCollection<GroupByColumnSyntax> Columns
        {
            get { return _columns; }
        }
    }
}