using System;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.Rearrangement
{
    public abstract class SeparatedSyntaxListRearranger<T> : SwappingRearranger<T>
        where T : SyntaxNode
    {
        protected override bool TryGetBeforeAfter(SyntaxTree syntaxTree, T node, int position, out TextSpan currentSpan, out TextSpan? beforeSpan, out TextSpan? afterSpan)
        {
            currentSpan = default(TextSpan);
            beforeSpan = null;
            afterSpan = null;

            var expressions = GetSyntaxList(node);
            if (expressions.Count == 1)
                return false;

            var index = expressions.IndexOf(node);
            var before = index == 0 ? null : expressions[index - 1];
            var after = index == expressions.Count - 1 ? null : expressions[index + 1];

            currentSpan = node.Span;
            beforeSpan = before == null ? (TextSpan?)null : before.Span;
            afterSpan = after == null ? (TextSpan?)null : after.Span;
            return true;
        }

        protected abstract SeparatedSyntaxList<T> GetSyntaxList(T node);
    }
}