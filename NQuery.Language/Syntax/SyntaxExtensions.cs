using System.Collections.Generic;
using System.Linq;

namespace NQuery.Language
{
    public static class SyntaxExtensions
    {
        public static SyntaxToken? WithParent(this SyntaxToken? token, SyntaxNode parent)
        {
            return token == null
                       ? (SyntaxToken?) null
                       : token.Value.WithParent(parent);
        }

        public static IList<SyntaxToken> WithParent(this IEnumerable<SyntaxToken> tokens, SyntaxNode parent)
        {
            return tokens.Select(t => t.WithParent(parent)).ToArray();
        }
    }
}