using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class CommonTableExpressionColumnNameListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly IList<SyntaxToken> _columnNames;
        private readonly SyntaxToken _rightParentheses;

        public CommonTableExpressionColumnNameListSyntax(SyntaxToken leftParentheses, IList<SyntaxToken> columnNames, SyntaxToken rightParentheses)
        {
            _leftParentheses = leftParentheses;
            _columnNames = columnNames;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnNameList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            foreach (var columnName in _columnNames)
                yield return columnName;
            yield return _rightParentheses;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public IList<SyntaxToken> ColumnNames
        {
            get { return _columnNames; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }
}