using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CommonTableExpressionSyntax : SyntaxNode
    {
        private readonly SyntaxToken _recursiveKeyword;
        private readonly SyntaxToken _name;
        private readonly CommonTableExpressionColumnNameListSyntax _columnNameList;
        private readonly SyntaxToken _asKeyword;
        private readonly SyntaxToken _leftParenthesis;
        private readonly QuerySyntax _query;
        private readonly SyntaxToken _rightParenthesis;

        internal CommonTableExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken recursiveKeyword, SyntaxToken name, CommonTableExpressionColumnNameListSyntax columnNameList, SyntaxToken asKeyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _recursiveKeyword = recursiveKeyword;
            _name = name;
            _columnNameList = columnNameList;
            _asKeyword = asKeyword;
            _leftParenthesis = leftParenthesis;
            _query = query;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            if (_recursiveKeyword != null)
                yield return _recursiveKeyword;
            yield return _name;
            if (_columnNameList != null)
                yield return _columnNameList;
            yield return _asKeyword;
            yield return _leftParenthesis;
            yield return _query;
            yield return _rightParenthesis;
        }

        public SyntaxToken RecursiveKeyword
        {
            get { return _recursiveKeyword; }
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }

        public CommonTableExpressionColumnNameListSyntax ColumnNameList
        {
            get { return _columnNameList; }
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
    }
}