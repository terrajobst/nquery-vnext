using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CommonTableExpressionColumnNameListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly IList<SyntaxToken> _columnNames;
        private readonly SyntaxToken _rightParenthesis;

        public CommonTableExpressionColumnNameListSyntax(SyntaxToken leftParenthesis, IList<SyntaxToken> columnNames, SyntaxToken rightParenthesis)
        {
            _leftParenthesis = leftParenthesis;
            _columnNames = columnNames;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnNameList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParenthesis;
            foreach (var columnName in _columnNames)
                yield return columnName;
            yield return _rightParenthesis;
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public IList<SyntaxToken> ColumnNames
        {
            get { return _columnNames; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }
}