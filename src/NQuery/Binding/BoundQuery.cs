using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundQuery : BoundNode
    {
        public BoundQuery(BoundRelation relation, IEnumerable<QueryColumnInstanceSymbol> output)
        {
            Relation = relation;
            OutputColumns = output.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.Query; }
        }

        public ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }

        public BoundRelation Relation { get; }
    }
}