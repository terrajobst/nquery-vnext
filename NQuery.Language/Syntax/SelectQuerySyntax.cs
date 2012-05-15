using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language
{
    public sealed class SelectQuerySyntax : QuerySyntax
    {
        private readonly SyntaxToken _selectKeyword;
        private readonly SyntaxToken? _distinctAllKeyword;
        private readonly TopClauseSyntax _topClause;
        private readonly ReadOnlyCollection<SelectColumnSyntax> _selectColumns;
        private readonly FromClauseSyntax _fromClause;
        private readonly WhereClauseSyntax _whereClause;
        private readonly GroupByClauseSyntax _groupByClause;
        private readonly HavingClauseSyntax _havingClause;

        public SelectQuerySyntax(SyntaxToken selectKeyword, SyntaxToken? distinctAllKeyword, TopClauseSyntax topClause, IList<SelectColumnSyntax> selectColumns, FromClauseSyntax fromClause, WhereClauseSyntax whereClause, GroupByClauseSyntax groupByClause, HavingClauseSyntax havingClause)
        {
            _selectKeyword = selectKeyword;
            _distinctAllKeyword = distinctAllKeyword;
            _topClause = topClause;
            _selectColumns = new ReadOnlyCollection<SelectColumnSyntax>(selectColumns);
            _fromClause = fromClause;
            _whereClause = whereClause;
            _groupByClause = groupByClause;
            _havingClause = havingClause;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SelectQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _selectKeyword;
            if (_distinctAllKeyword != null)
                yield return _distinctAllKeyword.Value;
            if (_topClause != null)
                yield return _topClause;
            foreach (var selectColumn in _selectColumns)
                yield return selectColumn;
            if (_fromClause != null)
                yield return _fromClause;
            if (_whereClause != null)
                yield return _whereClause;
            if (_groupByClause != null)
                yield return _groupByClause;
            if (_havingClause != null)
                yield return _havingClause;
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

        public ReadOnlyCollection<SelectColumnSyntax> SelectColumns
        {
            get { return _selectColumns; }
        }

        public FromClauseSyntax FromClause
        {
            get { return _fromClause; }
        }

        public WhereClauseSyntax WhereClause
        {
            get { return _whereClause; }
        }

        public GroupByClauseSyntax GroupByClause
        {
            get { return _groupByClause; }
        }

        public HavingClauseSyntax HavingClause
        {
            get { return _havingClause; }
        }
    }
}