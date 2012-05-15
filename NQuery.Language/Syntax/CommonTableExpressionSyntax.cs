using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CommonTableExpressionSyntax : SyntaxNode
    {
        private readonly SyntaxToken _identifer;
        private readonly CommonTableExpressionColumnNameListSyntax _commonTableExpressionColumnNameList;
        private readonly SyntaxToken _asKeyword;
        private readonly SyntaxToken _leftParentheses;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParentheses;
        private readonly SyntaxToken? _commaToken;

        public CommonTableExpressionSyntax(SyntaxToken identifer, CommonTableExpressionColumnNameListSyntax commonTableExpressionColumnNameList, SyntaxToken asKeyword, SyntaxToken leftParentheses, QuerySyntax query, SyntaxToken rightParentheses, SyntaxToken? commaToken)
        {
            _identifer = identifer;
            _commonTableExpressionColumnNameList = commonTableExpressionColumnNameList;
            _asKeyword = asKeyword;
            _leftParentheses = leftParentheses;
            _query = query;
            _rightParentheses = rightParentheses;
            _commaToken = commaToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifer;
            if (_commonTableExpressionColumnNameList != null)
                yield return _commonTableExpressionColumnNameList;
            yield return _asKeyword;
            yield return _leftParentheses;
            yield return _query;
            yield return _rightParentheses;
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

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }
}