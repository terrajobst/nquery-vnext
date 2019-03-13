#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Syntax
{
    public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
    {
        internal SkippedTokensTriviaSyntax(SyntaxTree syntaxTree, IEnumerable<SyntaxToken> tokens)
            : base(syntaxTree)
        {
            Tokens = tokens.ToImmutableArray();
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SkippedTokensTrivia; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            return Tokens.Select(token => (SyntaxNodeOrToken)token);
        }

        public ImmutableArray<SyntaxToken> Tokens { get; }
    }
}