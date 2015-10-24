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

            // Instantiate CTEs
            yield return new CommonTableExpressionInstantiator();

            // Expand full outer joins
            yield return new FullOuterJoinExpander();

            // Expand subqueries
            yield return new SubqueryExpander();

            // TODO: semi join simplification
            // TODO: decorrelation

            yield return new OuterJoinRemover();

            // selection pushing
            yield return new SelectionPusher();

            // join linearization
            yield return new JoinLinearizer();

            // TODO: outer join reordering
            // TODO: join order optimization
            // TODO: at most one row reordering
            // TODO: push computations
            
            // physical op choosing
            yield return new HashMatchPhysicalOperatorChooser();
            yield return new AggregationPhysicalOperatorChooser();
            yield return new ConcatenationPhysicalOperatorChooser();

            // TODO: null scan optimization
        }
    }
}