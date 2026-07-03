namespace NQuery.CodeAnalysis.Binding;

// A CROSS/OUTER APPLY: the right table reference is evaluated for each row of the left and
// may reference the left's columns (the correlation). Unlike a join, the binder puts the
// left's tables in scope while binding the right, so that correlation can resolve. The
// algebrizer lowers this into a dependent join (LogicalApply), which decorrelation turns
// back into an ordinary join when the right turns out not to depend on the left.
//
// CROSS APPLY is an inner apply (a left row with no matching right rows is dropped); OUTER
// APPLY is a left-outer apply (such a left row survives, null-padded). The binder only ever
// produces these two apply kinds.
internal sealed class BoundApplyTableReference : BoundTableReference
{
    public BoundApplyTableReference(BoundJoinType joinType, BoundTableReference left, BoundTableReference right)
    {
        ThrowIfNull(left);
        ThrowIfNull(right);

        JoinType = joinType;
        Left = left;
        Right = right;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ApplyTableReference;

    public BoundJoinType JoinType { get; }

    public BoundTableReference Left { get; }

    public BoundTableReference Right { get; }

    public override IEnumerable<IBoundValue> GetDefinedValues()
    {
        return Left.GetDefinedValues().Concat(Right.GetDefinedValues());
    }

    public override IEnumerable<IBoundValue> GetOutputValues()
    {
        return Left.GetOutputValues().Concat(Right.GetOutputValues());
    }
}
