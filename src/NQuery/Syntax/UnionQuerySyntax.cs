using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class UnionQuerySyntax : QuerySyntax
    {
        private readonly QuerySyntax _leftQuery;
        private readonly SyntaxToken _unionKeyword;
        private readonly SyntaxToken _allKeyword;
        private readonly QuerySyntax _rightQuery;

        internal UnionQuerySyntax(SyntaxTree syntaxTree, QuerySyntax leftQuery, SyntaxToken unionKeyword, SyntaxToken allKeyword, QuerySyntax rightQuery)
            : base(syntaxTree)
        {
            _leftQuery = leftQuery;
            _unionKeyword = unionKeyword;
            _allKeyword = allKeyword;
            _rightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.UnionQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _leftQuery;
            yield return _unionKeyword;
            if (_allKeyword != null)
                yield return _allKeyword;
            yield return _rightQuery;
        }

        public QuerySyntax LeftQuery
        {
            get { return _leftQuery; }
        }

        public SyntaxToken UnionKeyword
        {
            get { return _unionKeyword; }
        }

        public SyntaxToken AllKeyword
        {
            get { return _allKeyword; }
        }

        public QuerySyntax RightQuery
        {
            get { return _rightQuery; }
        }
    }
}