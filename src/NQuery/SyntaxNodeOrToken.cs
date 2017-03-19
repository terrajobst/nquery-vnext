using System;

using NQuery.Text;

namespace NQuery
{
    public struct SyntaxNodeOrToken
    {
        private readonly SyntaxNode _syntaxNode;
        private readonly SyntaxToken _syntaxToken;

        internal SyntaxNodeOrToken(SyntaxToken syntaxToken)
        {
            _syntaxToken = syntaxToken;
            _syntaxNode = null;
        }

        internal SyntaxNodeOrToken(SyntaxNode syntaxNode)
        {
            _syntaxToken = null;
            _syntaxNode = syntaxNode;
        }

        public bool IsToken => !IsNode;

        public bool IsNode => _syntaxNode != null;

        public SyntaxToken AsToken()
        {
            return _syntaxToken;
        }

        public SyntaxNode AsNode()
        {
            return _syntaxNode;
        }

        public bool IsEquivalentTo(SyntaxNodeOrToken other)
        {
            return SyntaxTreeEquivalence.AreEquivalent(this, other);
        }

        public SyntaxNode Parent => IsNode ? AsNode().Parent : AsToken().Parent;

        public SyntaxTree SyntaxTree => Parent.SyntaxTree;

        public SyntaxKind Kind => IsNode ? AsNode().Kind : AsToken().Kind;

        public TextSpan Span => IsNode ? AsNode().Span : AsToken().Span;

        public TextSpan FullSpan => IsNode ? AsNode().FullSpan : AsToken().FullSpan;

        public bool IsMissing => IsNode ? AsNode().IsMissing : AsToken().IsMissing;

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