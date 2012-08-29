using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class AliasSyntax : SyntaxNode
    {
        private readonly SyntaxToken? _asKeyword;
        private readonly SyntaxToken _identifier;

        public AliasSyntax(SyntaxToken? asKeyword, SyntaxToken identifier)
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
                yield return _asKeyword.Value;
            yield return _identifier;
        }

        public SyntaxToken? AsKeyword
        {
            get { return _asKeyword; }
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }
    }
}