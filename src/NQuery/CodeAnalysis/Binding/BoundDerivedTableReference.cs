using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundDerivedTableReference : BoundTableReference
{
    public BoundDerivedTableReference(TableInstanceSymbol tableInstance, BoundQuery query)
    {
        ThrowIfNull(tableInstance);
        ThrowIfNull(query);

        TableInstance = tableInstance;
        Query = query;
    }

    public override BoundNodeKind Kind => BoundNodeKind.DerivedTableReference;

    public TableInstanceSymbol TableInstance { get; }

    public BoundQuery Query { get; }

    public override IEnumerable<IBoundValue> GetDefinedValues()
    {
        return GetOutputValues();
    }

    public override IEnumerable<IBoundValue> GetOutputValues()
    {
        return Query.OutputColumns.Select(c => c.BoundValue);
    }
}
