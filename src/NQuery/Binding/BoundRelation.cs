namespace NQuery.Binding
{
    internal abstract class BoundRelation : BoundNode
    {
        public abstract IEnumerable<ValueSlot> GetDefinedValues();
        public abstract IEnumerable<ValueSlot> GetOutputValues();
    }
}