using System.Collections.Immutable;

using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

// Base of the syntax-shaped query tree the binder produces. It does not carry a
// relational algebra tree -- relational lowering is the algebrizer's job. All that is
// common across query forms is the set of output columns (name + value slot) the query
// exposes.
internal abstract class BoundQuery : BoundNode
{
    public abstract ImmutableArray<QueryColumnInstanceSymbol> OutputColumns { get; }
}
