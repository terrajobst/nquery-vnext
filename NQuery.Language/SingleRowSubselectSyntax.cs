using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class SingleRowSubselectSyntax : SubselectExpressionSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;

        public SingleRowSubselectSyntax(SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses)
        {
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SingleRowSubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
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