using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language
{
    public sealed class SelectClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _selectKeyword;
        private readonly SyntaxToken? _distinctAllKeyword;
        private readonly TopClauseSyntax _topClause;
        private readonly ReadOnlyCollection<SelectColumnSyntax> _columns;

        public SelectClauseSyntax(SyntaxToken selectKeyword, SyntaxToken? distinctAllKeyword, TopClauseSyntax topClause, IList<SelectColumnSyntax> selectColumns)
        {
            _selectKeyword = selectKeyword;
            _distinctAllKeyword = distinctAllKeyword;
            _topClause = topClause;
            _columns = new ReadOnlyCollection<SelectColumnSyntax>(selectColumns);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SelectClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _selectKeyword;
            if (_distinctAllKeyword != null)
                yield return _distinctAllKeyword.Value;
            if (_topClause != null)
                yield return _topClause;
            foreach (var selectColumn in _columns)
                yield return selectColumn;
        }

        public SyntaxToken SelectKeyword
        {
            get { return _selectKeyword; }
        }

        public SyntaxToken? DistinctAllKeyword
        {
            get { return _distinctAllKeyword; }
        }

        public TopClauseSyntax TopClause
        {
            get { return _topClause; }
        }

        public ReadOnlyCollection<SelectColumnSyntax> Columns
        {
            get { return _columns; }
        }
    }
}