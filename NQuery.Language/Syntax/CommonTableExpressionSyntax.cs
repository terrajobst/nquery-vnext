using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CommonTableExpressionSyntax : SyntaxNode
    {
        private readonly SyntaxToken _identifer;
        private readonly CommonTableExpressionColumnNameListSyntax _commonTableExpressionColumnNameList;
        private readonly SyntaxToken _asKeyword;
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;
        private readonly SyntaxToken? _commaToken;

        public CommonTableExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifer, CommonTableExpressionColumnNameListSyntax commonTableExpressionColumnNameList, SyntaxToken asKeyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis, SyntaxToken? commaToken)
            : base(syntaxTree)
        {
            _identifer = identifer.WithParent(this);
            _commonTableExpressionColumnNameList = commonTableExpressionColumnNameList;
            _asKeyword = asKeyword.WithParent(this);
            _leftParenthesis = leftParenthesis.WithParent(this);
            _query = query;
            _rightParenthesis = rightParenthesis.WithParent(this);
            _commaToken = commaToken.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _identifer;
            if (_commonTableExpressionColumnNameList != null)
                yield return _commonTableExpressionColumnNameList;
            yield return _asKeyword;
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
            if (_commaToken != null)
                yield return _commaToken.Value;
        }

        public SyntaxToken Identifer
        {
            get { return _identifer; }
        }

        public CommonTableExpressionColumnNameListSyntax CommonTableExpressionColumnNameList
        {
            get { return _commonTableExpressionColumnNameList; }
        }

        public SyntaxToken AsKeyword
        {
            get { return _asKeyword; }
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

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }
}