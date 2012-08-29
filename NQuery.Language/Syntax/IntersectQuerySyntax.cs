using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class IntersectQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _leftQuery;
        private readonly SyntaxToken _intersectKeyword;
        private readonly QuerySyntax _rightQuery;

        public IntersectQuerySyntax(QuerySyntax leftQuery, SyntaxToken intersectKeyword, QuerySyntax rightQuery)
        {
            _leftQuery = leftQuery;
            _intersectKeyword = intersectKeyword;
            _rightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.IntersectQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _leftQuery;
            yield return _intersectKeyword;
            yield return _rightQuery;
        }

        public QuerySyntax LeftQuery
        {
            get { return _leftQuery; }
        }

        public SyntaxToken IntersectKeyword
        {
            get { return _intersectKeyword; }
        }

        public QuerySyntax RightQuery
        {
            get { return _rightQuery; }
        }
    }
}