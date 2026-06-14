using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    // Base of the syntax-shaped query tree the AlgebraBinding binder produces. Unlike the
    // legacy binder, it does not carry a relational algebra tree -- relational lowering is
    // the algebrizer's job. All that is common across query forms is the set of output
    // columns (name + value slot) the query exposes.
    internal abstract class BoundQuery : BoundNode
    {
        public abstract ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
    }
}
