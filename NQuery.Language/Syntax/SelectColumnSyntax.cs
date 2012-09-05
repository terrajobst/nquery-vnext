using System;

namespace NQuery.Language
{
    public abstract class SelectColumnSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _commaToken;

        protected SelectColumnSyntax(SyntaxTree syntaxTree, SyntaxToken? commaToken)
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