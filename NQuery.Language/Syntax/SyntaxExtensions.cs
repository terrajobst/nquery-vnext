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

        public static SeparatedSyntaxList<TNode> WithParent<TNode>(this SeparatedSyntaxList<TNode> list, SyntaxNode parent)
            where TNode : SyntaxNode
        {
            var fixedTokens = (from nodeOrToken in list.GetWithSeparators()
                               let fixedNodeOrToken = nodeOrToken.IsNode
                                                          ? nodeOrToken
                                                          : nodeOrToken.AsToken().WithParent(parent)
                               select fixedNodeOrToken).ToArray();
            return new SeparatedSyntaxList<TNode>(fixedTokens);
        }
    }
}