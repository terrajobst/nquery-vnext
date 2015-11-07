using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class IntersectQuerySyntax : QuerySyntax
    {
        internal IntersectQuerySyntax(SyntaxTree syntaxTree, QuerySyntax leftQuery, SyntaxToken intersectKeyword, QuerySyntax rightQuery)
            : base(syntaxTree)
        {
            LeftQuery = leftQuery;
            IntersectKeyword = intersectKeyword;
            RightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.IntersectQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftQuery;
            yield return IntersectKeyword;
            yield return RightQuery;
        }

        public QuerySyntax LeftQuery { get; }

        public SyntaxToken IntersectKeyword { get; }

        public QuerySyntax RightQuery { get; }
    }
}