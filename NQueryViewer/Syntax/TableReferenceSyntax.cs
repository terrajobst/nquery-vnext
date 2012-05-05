using System;

namespace NQueryViewer.Syntax
{
    public abstract class TableReferenceSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _commaToken;

        protected TableReferenceSyntax(SyntaxToken? commaToken)
        {
            _commaToken = commaToken;
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }
}