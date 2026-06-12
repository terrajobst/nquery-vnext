namespace NQuery.AlgebraBinding
{
    // A bound FROM-clause table reference. Unlike the legacy binder, the AlgebraBinding
    // binder keeps the FROM clause as a syntax-shaped table-reference tree (named table,
    // derived table, join) rather than lowering it into relational algebra. Lowering to
    // relations/logical operators is the algebrizer's job.
    internal abstract class BoundTableReference : BoundNode
    {
        public abstract IEnumerable<ValueSlot> GetDefinedValues();

        public abstract IEnumerable<ValueSlot> GetOutputValues();
    }
}
