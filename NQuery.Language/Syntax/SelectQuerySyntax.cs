using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class SelectQuerySyntax : QuerySyntax
    {
        private readonly SelectClauseSyntax _selectClause;
        private readonly FromClauseSyntax _fromClause;
        private readonly WhereClauseSyntax _whereClause;
        private readonly GroupByClauseSyntax _groupByClause;
        private readonly HavingClauseSyntax _havingClause;

        public SelectQuerySyntax(SyntaxTree syntaxTree, SelectClauseSyntax selectClause, FromClauseSyntax fromClause, WhereClauseSyntax whereClause, GroupByClauseSyntax groupByClause, HavingClauseSyntax havingClause)
            : base(syntaxTree)
        {
            _selectClause = selectClause;
            _fromClause = fromClause;
            _whereClause = whereClause;
            _groupByClause = groupByClause;
            _havingClause = havingClause;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SelectQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _selectClause;
            if (_fromClause != null)
                yield return _fromClause;
            if (_whereClause != null)
                yield return _whereClause;
            if (_groupByClause != null)
                yield return _groupByClause;
            if (_havingClause != null)
                yield return _havingClause;
        }

        public SelectClauseSyntax SelectClause
        {
            get { return _selectClause; }
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