using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class AllAnySubselectSyntax : SubselectExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _operatorToken;
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;

        public AllAnySubselectSyntax(ExpressionSyntax left, SyntaxToken operatorToken, SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses)
        {
            _left = left;
            _operatorToken = operatorToken;
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.AllAnySubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            yield return _operatorToken;
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken OperatorToken
        {
            get { return _operatorToken; }
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