using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Syntax
{
    public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
    {
        private readonly ReadOnlyCollection<SyntaxToken> _tokens;

        public SkippedTokensTriviaSyntax(SyntaxTree syntaxTree, IEnumerable<SyntaxToken> tokens)
            : base(syntaxTree)
        {
            _tokens = new ReadOnlyCollection<SyntaxToken>(tokens.ToArray());
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SkippedTokensTrivia; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            return _tokens.Select(token => (SyntaxNodeOrToken)token);
        }

        public ReadOnlyCollection<SyntaxToken> Tokens
        {
            get { return _tokens; }
        }
    }
}