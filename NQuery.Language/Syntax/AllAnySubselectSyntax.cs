using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class AllAnySubselectSyntax : SubselectExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _operatorToken;
        private readonly SyntaxToken _keyword;
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;

        public AllAnySubselectSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, SyntaxToken keyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _left = left;
            _operatorToken = operatorToken;
            _keyword = keyword;
            _leftParenthesis = leftParenthesis;
            _query = query;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            // TODO: May be we should have different values for ALL, ANY, and SOME?
            get { return SyntaxKind.AllAnySubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _left;
            yield return _operatorToken;
            yield return _keyword;
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken OperatorToken
        {
            get { return _operatorToken; }
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