#nullable enable

using System.Collections.Immutable;

using NQuery.Refactor.Algebra;

namespace NQuery.Refactor.Optimization
{
    // Decorrelation by pushing Apply down: an Apply whose right side no longer
    // depends on the left becomes an ordinary join. Two rules so far:
    //
    //   * base case -- Apply(L, R) with R independent of L  ->  Join(L, R) (no
    //     condition); this fully decorrelates uncorrelated subqueries.
    //   * correlated filter -- Apply(L, Filter(R', p)) with R' independent of L
    //     ->  Join(L, R') ON p; the correlated predicate becomes the join
    //     condition. This decorrelates EXISTS / scalar subqueries over a filtered
    //     source.
    //
    // The apply kind maps to the join type (Semi/AntiSemi carry their probe slot).
    // Anything not matched is left as an Apply -- still correct (executable as a
    // correlated dependent join); pushing through aggregates and joins (with the
    // group-key augmentation that avoids the count bug) is future work.
    internal sealed class ApplyPushdown : LogicalOperatorRewriter
    {
        public static readonly ApplyPushdown Instance = new();

        private ApplyPushdown()
        {
        }

        protected override LogicalOperator RewriteApply(LogicalApply node)
        {
            // Rewrite children first, so nested applies are decorrelated bottom-up.
            var rewritten = base.RewriteApply(node);
            return rewritten is LogicalApply apply ? Decorrelate(apply) : rewritten;
        }

        private static LogicalOperator Decorrelate(LogicalApply apply)
        {
            // A guarded apply (a CASE-branch subquery whose evaluation is conditional)
            // is left as nested loops: the executor's passthru handling skips the right
            // for guarded rows, which a decorrelated join would lose.
            if (apply.Passthru is not null)
                return apply;

            // Base case: no outer references, so the right no longer reaches into the
            // left -- it is already an ordinary join.
            if (apply.OuterReferences.IsEmpty)
                return ToJoin(apply, apply.Right, ImmutableArray<LogicalExpression>.Empty);

            switch (apply.Right)
            {
                // An existence test ignores the right's columns, so a project there
                // is irrelevant -- drop it and continue.
                case LogicalProject project when IsSemi(apply.ApplyKind):
                    return Decorrelate(new LogicalApply(apply.ApplyKind, apply.Left, project.Input, apply.Probe));

                // Otherwise push the apply below the project, keeping the left's
                // columns in scope (Apply(L, π(R)) == π(Apply(L, R)) over L ∪ outs).
                case LogicalProject project:
                    var inner = Decorrelate(new LogicalApply(apply.ApplyKind, apply.Left, project.Input, apply.Probe));
                    var outputs = apply.Left.OutputValueSlots.Concat(project.Outputs).ToImmutableArray();
                    return new LogicalProject(inner, outputs);

                // The correlated predicate becomes the join condition once the rest
                // of the right side is independent of the left.
                case LogicalFilter filter when !DependsOnLeft(filter.Input, apply.Left):
                    return ToJoin(apply, filter.Input, filter.Conditions);

                // TODO: Push Apply through GroupBy (Apply(L, Agg(R)) -> Agg-with-group-on-
                //       L's-key over Apply(L, R)), the "magic decorrelation" rule. Until
                //       then a guarded scalar subquery -- Apply(L, Assert(Aggregate(...)))
                //       from Algebrizer.GuardSingleRow -- falls through here and runs as
                //       correlated nested loops (correct, but a re-scan per left row).
                default:
                    return apply;
            }
        }

        private static bool IsSemi(LogicalApplyKind kind)
        {
            return kind is LogicalApplyKind.LeftSemi or LogicalApplyKind.LeftAntiSemi;
        }

        private static bool DependsOnLeft(LogicalOperator right, LogicalOperator left)
        {
            var references = LogicalSlotReferenceFinder.FindReferencedSlots(right);
            return left.DefinedValueSlots.Any(references.Contains);
        }

        private static LogicalOperator ToJoin(LogicalApply apply, LogicalOperator right, ImmutableArray<LogicalExpression> conditions)
        {
            var joinKind = apply.ApplyKind switch
            {
                LogicalApplyKind.Inner => LogicalJoinKind.Inner,
                LogicalApplyKind.LeftOuter => LogicalJoinKind.LeftOuter,
                LogicalApplyKind.LeftSemi => LogicalJoinKind.LeftSemi,
                LogicalApplyKind.LeftAntiSemi => LogicalJoinKind.LeftAntiSemi,
                _ => throw ExceptionBuilder.UnexpectedValue(apply.ApplyKind)
            };

            return new LogicalJoin(joinKind, apply.Left, right, conditions, apply.Probe, passthruPredicate: null);
        }
    }
}
