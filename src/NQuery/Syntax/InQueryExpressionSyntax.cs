using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class InQueryExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _notKeyword;
        private readonly SyntaxToken _inKeyword;
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;

        public InQueryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken notKeyword, SyntaxToken inKeyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _expression = expression;
            _notKeyword = notKeyword;
            _inKeyword = inKeyword;
            _leftParenthesis = leftParenthesis;
            _query = query;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InQueryExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _expression;
            if (_notKeyword != null)
                yield return _notKeyword;
            yield return _inKeyword;
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken InKeyword
        {
            get { return _inKeyword; }
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