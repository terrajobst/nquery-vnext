using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class ConcatenationPhysicalOperatorChooser : BoundTreeRewriter
    {
        protected override BoundRelation RewriteUnionRelation(BoundUnionRelation node)
        {
            var inputs = RewriteRelations(node.Inputs);
            var values = node.DefinedValues;
            var concatenation = new BoundConcatenationRelation(inputs, values);
            if (node.IsUnionAll)
                return concatenation;

            var sortedValues = values.Zip(node.Comparers, (v, c) => new BoundComparedValue(v.ValueSlot, c));
            return new BoundSortRelation(true, concatenation, sortedValues);
        }
    }
}