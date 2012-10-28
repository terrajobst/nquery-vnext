using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ExceptQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _leftQuery;
        private readonly SyntaxToken _exceptKeyword;
        private readonly QuerySyntax _rightQuery;

        public ExceptQuerySyntax(SyntaxTree syntaxTree, QuerySyntax leftQuery, SyntaxToken exceptKeyword, QuerySyntax rightQuery)
            : base(syntaxTree)
        {
            _leftQuery = leftQuery;
            _exceptKeyword = exceptKeyword;
            _rightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExceptQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _leftQuery;
            yield return _exceptKeyword;
            yield return _rightQuery;
        }

        public QuerySyntax LeftQuery
        {
            get { return _leftQuery; }
        }

        public SyntaxToken ExceptKeyword
        {
            get { return _exceptKeyword; }
        }

        public QuerySyntax RightQuery
        {
            get { return _rightQuery; }
        }
    }
}