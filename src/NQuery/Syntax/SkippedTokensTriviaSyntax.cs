using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Syntax
{
    public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
    {
        private readonly ImmutableArray<SyntaxToken> _tokens;

        public SkippedTokensTriviaSyntax(SyntaxTree syntaxTree, IEnumerable<SyntaxToken> tokens)
            : base(syntaxTree)
        {
            _tokens = tokens.ToImmutableArray();
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SkippedTokensTrivia; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            return _tokens.Select(token => (SyntaxNodeOrToken)token);
        }

        public ImmutableArray<SyntaxToken> Tokens
        {
            get { return _tokens; }
        }
    }
}