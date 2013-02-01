using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class DerivedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;
        private readonly SyntaxToken _asKeyword;
        private readonly SyntaxToken _name;

        public DerivedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis, SyntaxToken asKeyword, SyntaxToken name)
            : base(syntaxTree)
        {
            _leftParenthesis = leftParenthesis;
            _query = query;
            _rightParenthesis = rightParenthesis;
            _asKeyword = asKeyword;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.DerivedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
            if (_asKeyword != null)
                yield return _asKeyword;
            yield return _name;
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

        public SyntaxToken AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }
}