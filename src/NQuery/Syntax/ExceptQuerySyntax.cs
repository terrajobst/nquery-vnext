#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class ExceptQuerySyntax : QuerySyntax
    {
        internal ExceptQuerySyntax(SyntaxTree syntaxTree, QuerySyntax leftQuery, SyntaxToken exceptKeyword, QuerySyntax rightQuery)
            : base(syntaxTree)
        {
            LeftQuery = leftQuery;
            ExceptKeyword = exceptKeyword;
            RightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExceptQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftQuery;
            yield return ExceptKeyword;
            yield return RightQuery;
        }

        public QuerySyntax LeftQuery { get; }

        public SyntaxToken ExceptKeyword { get; }

        public QuerySyntax RightQuery { get; }
    }
}