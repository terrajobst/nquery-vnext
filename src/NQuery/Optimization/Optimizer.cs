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

            // semi join simplification
            // decorrelation
            // outer join removal

            // selection pushing
            yield return new SelectionPusher();

            // join linearization
            yield return new JoinLinearizer();

            // outer join reordering
            // join order optimization
            // at most one row reordering
            // push computations
            // physical join op choosing
            // null scan optimization
            // full outer join expansion
        }
    }
}