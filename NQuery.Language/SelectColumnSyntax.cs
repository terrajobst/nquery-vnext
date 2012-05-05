using System;

namespace NQuery.Language
{
    public abstract class SelectColumnSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _commaToken;

        protected SelectColumnSyntax(SyntaxToken? commaToken)
        {
            _commaToken = commaToken;
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }
}