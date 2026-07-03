using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundSelectColumn : BoundNode
{
    public BoundSelectColumn(QueryColumnInstanceSymbol column)
    {
        ThrowIfNull(column);

        Column = column;
    }

    public override BoundNodeKind Kind
    {
        get { return BoundNodeKind.SelectColumn; }
    }

    public QueryColumnInstanceSymbol Column { get; }
}
