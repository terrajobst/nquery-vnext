using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class DerivedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;
        private readonly SyntaxToken? _asKeyword;
        private readonly SyntaxToken _name;

        public DerivedTableReferenceSyntax(SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses, SyntaxToken? asKeyword, SyntaxToken name, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
            _asKeyword = asKeyword;
            _name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.DerivedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
            if (_asKeyword != null)
                yield return _asKeyword.Value;
            yield return _name;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public QuerySyntax Query
        {
            get { return _query; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }

        public SyntaxToken? AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }
    }
}