using System;
using System.Linq;

namespace NQuery.Authoring.Rearrangement
{
    public abstract class Rearranger<T> : IRearranger
        where T: SyntaxNode
    {
        public Arrangement GetArrangement(SyntaxTree syntaxTree, int position)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var node = token.Parent
                .AncestorsAndSelf()
                .OfType<T>()
                .FirstOrDefault();

            return node == null
                ? null
                : GetArrangement(syntaxTree, node, position);
        }

        protected abstract Arrangement GetArrangement(SyntaxTree syntaxTree, T node, int position);
    }
}