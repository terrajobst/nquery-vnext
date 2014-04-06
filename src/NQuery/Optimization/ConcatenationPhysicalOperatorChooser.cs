using System;
using System.Collections;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class ConcatenationPhysicalOperatorChooser : BoundTreeRewriter
    {
        protected override BoundRelation RewriteUnionRelation(BoundUnionRelation node)
        {
            var inputs = RewriteRelations(node.Inputs);
            var values = RewriteUnifiedValues(node.DefinedValues);
            var concatenation = new BoundConcatenationRelation(inputs, values);
            if (node.IsUnionAll)
                return concatenation;

            var sortedValues = concatenation.GetOutputValues().Select(v => new BoundSortedValue(v, Comparer.Default));
            return new BoundSortRelation(true, concatenation, sortedValues);
        }
    }
}