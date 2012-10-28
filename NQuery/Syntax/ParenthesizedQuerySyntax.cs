using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ParenthesizedQuerySyntax : QuerySyntax
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;

        public ParenthesizedQuerySyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _leftParenthesis = leftParenthesis;
            _query = query;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }
}