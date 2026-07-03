namespace NQuery.CodeAnalysis.Binding;

// A FROM-clause join (CROSS/INNER/LEFT/RIGHT/FULL). The binder only ever produces these
// join kinds; semi/anti joins, probes and passthru predicates are introduced later by the
// algebrizer, so they have no place on a syntax-shaped table reference.
internal sealed class BoundJoinTableReference : BoundTableReference
{
    public BoundJoinTableReference(BoundJoinType joinType, BoundTableReference left, BoundTableReference right, BoundExpression? condition)
    {
        ThrowIfNull(left);
        ThrowIfNull(right);

        JoinType = joinType;
        Left = left;
        Right = right;
        Condition = condition;
    }

    public override BoundNodeKind Kind => BoundNodeKind.JoinTableReference;

    public BoundJoinType JoinType { get; }

    public BoundTableReference Left { get; }

    public BoundTableReference Right { get; }

    public BoundExpression? Condition { get; }

    public override IEnumerable<IBoundValue> GetDefinedValues()
    {
        return Left.GetDefinedValues().Concat(Right.GetDefinedValues());
    }

    public override IEnumerable<IBoundValue> GetOutputValues()
    {
        return Left.GetOutputValues().Concat(Right.GetOutputValues());
    }
}
