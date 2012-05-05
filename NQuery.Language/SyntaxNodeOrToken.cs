using System;

namespace NQuery.Language
{
    public struct SyntaxNodeOrToken
    {
        private readonly SyntaxNode _syntaxNode;
        private readonly SyntaxToken _syntaxToken;

        public SyntaxNodeOrToken(SyntaxToken syntaxToken)
        {
            _syntaxToken = syntaxToken;
            _syntaxNode = null;
        }

        public SyntaxNodeOrToken(SyntaxNode syntaxNode)
        {
            _syntaxToken = default(SyntaxToken);
            _syntaxNode = syntaxNode;
        }

        public bool IsToken
        {
            get { return !IsNode; }
        }

        public bool IsNode
        {
            get { return _syntaxNode != null; }
        }

        public SyntaxToken AsToken
        {
            get { return _syntaxToken; }
        }

        public SyntaxNode AsNode
        {
            get { return _syntaxNode; }
        }

        public TextSpan Span
        {
            get { return IsNode ? AsNode.Span : AsToken.Span; }
        }

        public TextSpan FullSpan
        {
            get { return IsNode ? AsNode.FullSpan : AsToken.FullSpan; }
        }

        public static implicit operator SyntaxNodeOrToken(SyntaxToken syntaxToken)
        {
            return new SyntaxNodeOrToken(syntaxToken);
        }

        public static implicit operator SyntaxNodeOrToken(SyntaxNode syntaxNode)
        {
            return new SyntaxNodeOrToken(syntaxNode);
        }
    }
}