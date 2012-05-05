using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ExistsSubselectSyntax : SubselectExpressionSyntax
    {
        private readonly SyntaxToken _existsKeyword;
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;

        public ExistsSubselectSyntax(SyntaxToken existsKeyword, SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses)
        {
            _existsKeyword = existsKeyword;
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExistsSubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _existsKeyword;
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
        }

        public SyntaxToken ExistsKeyword
        {
            get { return _existsKeyword; }
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
    }
}