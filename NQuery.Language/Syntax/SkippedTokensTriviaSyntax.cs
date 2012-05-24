using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Language
{
    public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
    {
        private readonly ReadOnlyCollection<SyntaxToken> _tokens;

        public SkippedTokensTriviaSyntax(IList<SyntaxToken> tokens)
        {
            _tokens = new ReadOnlyCollection<SyntaxToken>(tokens);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SkippedTokensTrivia; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            return _tokens.Select(token => (SyntaxNodeOrToken)token);
        }

        public ReadOnlyCollection<SyntaxToken> Tokens
        {
            get { return _tokens; }
        }
    }
}