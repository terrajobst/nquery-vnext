using System.Collections;
using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Refactor.Binding
{
    internal sealed class BoundIntersectOrExceptQuery : BoundQuery
    {
        public BoundIntersectOrExceptQuery(
            bool isIntersect,
            BoundQueryInput left,
            BoundQueryInput right,
            ImmutableArray<IComparer> comparers,
            ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
        {
            IsIntersect = isIntersect;
            Left = left;
            Right = right;
            Comparers = comparers;
            OutputColumns = outputColumns;
        }

        public override BoundNodeKind Kind => BoundNodeKind.IntersectOrExceptQuery;

        public bool IsIntersect { get; }

        public BoundQueryInput Left { get; }

        public BoundQueryInput Right { get; }

        public ImmutableArray<IComparer> Comparers { get; }

        public override ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
    }
}
