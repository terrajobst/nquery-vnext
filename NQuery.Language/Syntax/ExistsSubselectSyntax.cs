using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ExistsSubselectSyntax : SubselectExpressionSyntax
    {
        private readonly SyntaxToken _existsKeyword;
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;

        public ExistsSubselectSyntax(SyntaxToken existsKeyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
        {
            _existsKeyword = existsKeyword;
            _leftParenthesis = leftParenthesis;
            _query = query;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExistsSubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _existsKeyword;
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
        }

        public SyntaxToken ExistsKeyword
        {
            get { return _existsKeyword; }
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