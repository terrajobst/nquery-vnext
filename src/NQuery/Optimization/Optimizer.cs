using System.Collections.Generic;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal static class Optimizer
    {
        public static BoundQuery Optimize(BoundQuery query)
        {
            var optmizedRelation = Optimize(query.Relation);
            return new BoundQuery(optmizedRelation, query.OutputColumns);
        }

        private static BoundRelation Optimize(BoundRelation relation)
        {
            foreach (var step in GetOptimizationSteps())
                relation = step.RewriteRelation(relation);

            return relation;
        }

        public static IEnumerable<BoundTreeRewriter> GetOptimizationSteps()
        {
            // TODO: This shouldn't be necessary
            yield return new DerivedTableRemover();

            // TODO: semi join simplification
            // TODO: decorrelation
            // TODO: outer join removal

            // selection pushing
            yield return new SelectionPusher();

            // join linearization
            yield return new JoinLinearizer();

            // TODO: outer join reordering
            // TODO: join order optimization
            // TODO: at most one row reordering
            // TODO: push computations
            
            // physical join op choosing
            yield return new HashMatchPhysicalOperatorChooser(); ;
            yield return new AggregationPhysicalOperatorChooser();

            // TODO: null scan optimization
            // TODO: full outer join expansion
        }
    }
}