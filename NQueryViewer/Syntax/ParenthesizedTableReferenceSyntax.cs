using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class ParenthesizedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly TableReferenceSyntax _tableReference;
        private readonly SyntaxToken _rightParentheses;

        public ParenthesizedTableReferenceSyntax(SyntaxToken leftParentheses, TableReferenceSyntax tableReference, SyntaxToken rightParentheses, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _leftParentheses = leftParentheses;
            _tableReference = tableReference;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _tableReference;
            yield return _rightParentheses;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public TableReferenceSyntax TableReference
        {
            get { return _tableReference; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }
}