using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class DerivedTableRemover : BoundTreeRewriter
    {
        protected override BoundRelation RewriteDerivedTableRelation(BoundDerivedTableRelation node)
        {
            return node.Relation;
        }
    }
}