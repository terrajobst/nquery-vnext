using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class AllAnySubselectSyntax : SubselectExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _keyword;
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;

        public AllAnySubselectSyntax(ExpressionSyntax left, SyntaxToken keyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
        {
            _left = left;
            _keyword = keyword;
            _leftParenthesis = leftParenthesis;
            _query = query;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.AllAnySubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _left;
            yield return _keyword;
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken Keyword
        {
            get { return _keyword; }
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