using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class AliasSyntax : SyntaxNode
    {
        private readonly SyntaxToken _asKeyword;
        private readonly SyntaxToken _identifier;

        internal AliasSyntax(SyntaxTree syntaxTree, SyntaxToken asKeyword, SyntaxToken identifier)
            : base(syntaxTree)
        {
            _asKeyword = asKeyword;
            _identifier = identifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.Alias; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            if (_asKeyword != null)
                yield return _asKeyword;
            yield return _identifier;
        }

        public SyntaxToken AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }
    }
}