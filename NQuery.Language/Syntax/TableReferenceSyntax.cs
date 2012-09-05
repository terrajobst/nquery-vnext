using System;

namespace NQuery.Language
{
    public abstract class TableReferenceSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _commaToken;

        protected TableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken? commaToken)
            : base(syntaxTree)
        {
            _commaToken = commaToken.WithParent(this);
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }
}