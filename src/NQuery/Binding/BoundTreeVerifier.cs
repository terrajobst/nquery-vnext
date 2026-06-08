using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    // Verifies structural invariants of a bound relational tree that the type
    // system cannot enforce. The central invariant is value-slot resolution:
    //
    //   Every ValueSlot referenced by an expression or by a slot-typed member
    //   must be defined by the subtree that feeds the referencing node, or be
    //   a legitimate outer reference provided by an enclosing scope (the left
    //   side of a join for its right side, or the scope surrounding a subselect).
    //
    // This catches the failure modes of slot remapping during optimization
    // (e.g. Instantiator cloning a subtree): a reference left pointing at a slot
    // that is no longer defined (dangling), or at a slot that belongs to an
    // unrelated subtree (scope escape). Both otherwise surface only as silently
    // wrong results, or much later as a KeyNotFoundException in RowBufferAllocation.
    //
    // Scope flows through the visit methods as parameters, uniformly per group:
    // every relation handler takes the outer scope available for correlation,
    // and every expression handler takes the slots in scope at its use site
    // (with the outer scope already folded in) plus a context for diagnostics.
    internal sealed class BoundTreeVerifier
    {
        private static readonly FrozenSet<ValueSlot> None = FrozenSet<ValueSlot>.Empty;

        private readonly List<string> _problems = new();

        private BoundTreeVerifier()
        {
        }

        public static void Verify(BoundRelation relation)
        {
            ArgumentNullException.ThrowIfNull(relation);

            var verifier = new BoundTreeVerifier();
            verifier.VerifyRelation(relation, None);
            verifier.ThrowIfFailed();
        }

        public static void Verify(BoundQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var verifier = new BoundTreeVerifier();
            verifier.VerifyRelation(query.Relation, None);

            var output = Defined(query.Relation);
            foreach (var column in query.OutputColumns)
                verifier.CheckReference(column.ValueSlot, output, $"output column '{column.Name}'");

            verifier.ThrowIfFailed();
        }

        private void VerifyRelation(BoundRelation node, FrozenSet<ValueSlot> outer)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.EmptyRelation:
                    VerifyEmptyRelation((BoundEmptyRelation)node, outer);
                    break;
                case BoundNodeKind.ConstantRelation:
                    VerifyConstantRelation((BoundConstantRelation)node, outer);
                    break;
                case BoundNodeKind.TableRelation:
                    VerifyTableRelation((BoundTableRelation)node, outer);
                    break;
                case BoundNodeKind.DerivedTableRelation:
                    VerifyDerivedTableRelation((BoundDerivedTableRelation)node, outer);
                    break;
                case BoundNodeKind.JoinRelation:
                    VerifyJoinRelation((BoundJoinRelation)node, outer);
                    break;
                case BoundNodeKind.HashMatchRelation:
                    VerifyHashMatchRelation((BoundHashMatchRelation)node, outer);
                    break;
                case BoundNodeKind.ComputeRelation:
                    VerifyComputeRelation((BoundComputeRelation)node, outer);
                    break;
                case BoundNodeKind.FilterRelation:
                    VerifyFilterRelation((BoundFilterRelation)node, outer);
                    break;
                case BoundNodeKind.GroupByAndAggregationRelation:
                    VerifyGroupByAndAggregationRelation((BoundGroupByAndAggregationRelation)node, outer);
                    break;
                case BoundNodeKind.StreamAggregatesRelation:
                    VerifyStreamAggregatesRelation((BoundStreamAggregatesRelation)node, outer);
                    break;
                case BoundNodeKind.UnionRelation:
                    VerifyUnionRelation((BoundUnionRelation)node, outer);
                    break;
                case BoundNodeKind.ConcatenationRelation:
                    VerifyConcatenationRelation((BoundConcatenationRelation)node, outer);
                    break;
                case BoundNodeKind.IntersectOrExceptRelation:
                    VerifyIntersectOrExceptRelation((BoundIntersectOrExceptRelation)node, outer);
                    break;
                case BoundNodeKind.ProjectRelation:
                    VerifyProjectRelation((BoundProjectRelation)node, outer);
                    break;
                case BoundNodeKind.SortRelation:
                    VerifySortRelation((BoundSortRelation)node, outer);
                    break;
                case BoundNodeKind.TopRelation:
                    VerifyTopRelation((BoundTopRelation)node, outer);
                    break;
                case BoundNodeKind.AssertRelation:
                    VerifyAssertRelation((BoundAssertRelation)node, outer);
                    break;
                case BoundNodeKind.TableSpoolPusher:
                    VerifyTableSpoolPusher((BoundTableSpoolPusher)node, outer);
                    break;
                case BoundNodeKind.TableSpoolPopper:
                    VerifyTableSpoolPopper((BoundTableSpoolPopper)node, outer);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        // Leaves: they only originate slots, they reference none.

        private void VerifyEmptyRelation(BoundEmptyRelation node, FrozenSet<ValueSlot> outer)
        {
        }

        private void VerifyConstantRelation(BoundConstantRelation node, FrozenSet<ValueSlot> outer)
        {
        }

        private void VerifyTableRelation(BoundTableRelation node, FrozenSet<ValueSlot> outer)
        {
        }

        private void VerifyTableSpoolPopper(BoundTableSpoolPopper node, FrozenSet<ValueSlot> outer)
        {
        }

        private void VerifyFilterRelation(BoundFilterRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Input, outer);
            VerifyExpression(node.Condition, Available(outer, node.Input), "the filter condition");
        }

        private void VerifyAssertRelation(BoundAssertRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Input, outer);
            VerifyExpression(node.Condition, Available(outer, node.Input), "the assert condition");
        }

        private void VerifyComputeRelation(BoundComputeRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Input, outer);

            var available = Available(outer, node.Input);
            foreach (var value in node.DefinedValues)
                VerifyExpression(value.Expression, available, "a computed value");
        }

        private void VerifyProjectRelation(BoundProjectRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Input, outer);

            var available = Available(outer, node.Input);
            foreach (var output in node.Outputs)
                CheckReference(output, available, "a project output");
        }

        private void VerifySortRelation(BoundSortRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Input, outer);

            var available = Available(outer, node.Input);
            foreach (var sorted in node.SortedValues)
                CheckReference(sorted.ValueSlot, available, "a sort key");
        }

        private void VerifyTopRelation(BoundTopRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Input, outer);

            var available = Available(outer, node.Input);
            foreach (var tie in node.TieEntries)
                CheckReference(tie.ValueSlot, available, "a top tie key");
        }

        private void VerifyJoinRelation(BoundJoinRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Left, outer);

            // The right side may correlate to slots defined on the left.
            VerifyRelation(node.Right, Available(outer, node.Left));

            var available = Available(outer, node.Left, node.Right);
            CheckReference(node.Probe, available, "the join probe");
            VerifyExpression(node.Condition, available, "the join condition");
            VerifyExpression(node.PassthruPredicate, available, "the join passthru predicate");
        }

        private void VerifyHashMatchRelation(BoundHashMatchRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Build, outer);
            VerifyRelation(node.Probe, outer);

            CheckReference(node.BuildKey, Available(outer, node.Build), "the hash-match build key");
            CheckReference(node.ProbeKey, Available(outer, node.Probe), "the hash-match probe key");
            VerifyExpression(node.Remainder, Available(outer, node.Build, node.Probe), "the hash-match remainder");
        }

        private void VerifyGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyAggregation(node.Input, node.Groups, node.Aggregates, outer);
        }

        private void VerifyStreamAggregatesRelation(BoundStreamAggregatesRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyAggregation(node.Input, node.Groups, node.Aggregates, outer);
        }

        private void VerifyUnionRelation(BoundUnionRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyUnified(node.Inputs, node.DefinedValues, outer);
        }

        private void VerifyConcatenationRelation(BoundConcatenationRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyUnified(node.Inputs, node.DefinedValues, outer);
        }

        private void VerifyIntersectOrExceptRelation(BoundIntersectOrExceptRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Left, outer);
            VerifyRelation(node.Right, outer);
            VerifyArityMatch(node.Left, node.Right, "intersect/except");
        }

        private void VerifyDerivedTableRelation(BoundDerivedTableRelation node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Relation, outer);
        }

        private void VerifyTableSpoolPusher(BoundTableSpoolPusher node, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(node.Input, outer);
        }

        private void VerifyExpression(BoundExpression node, FrozenSet<ValueSlot> available, string context)
        {
            if (node is null)
                return;

            switch (node.Kind)
            {
                case BoundNodeKind.ValueSlotExpression:
                    VerifyValueSlotExpression((BoundValueSlotExpression)node, available, context);
                    break;
                case BoundNodeKind.UnaryExpression:
                    VerifyUnaryExpression((BoundUnaryExpression)node, available, context);
                    break;
                case BoundNodeKind.BinaryExpression:
                    VerifyBinaryExpression((BoundBinaryExpression)node, available, context);
                    break;
                case BoundNodeKind.FunctionInvocationExpression:
                    VerifyFunctionInvocationExpression((BoundFunctionInvocationExpression)node, available, context);
                    break;
                case BoundNodeKind.AggregateExpression:
                    VerifyAggregateExpression((BoundAggregateExpression)node, available, context);
                    break;
                case BoundNodeKind.PropertyAccessExpression:
                    VerifyPropertyAccessExpression((BoundPropertyAccessExpression)node, available, context);
                    break;
                case BoundNodeKind.MethodInvocationExpression:
                    VerifyMethodInvocationExpression((BoundMethodInvocationExpression)node, available, context);
                    break;
                case BoundNodeKind.ConversionExpression:
                    VerifyConversionExpression((BoundConversionExpression)node, available, context);
                    break;
                case BoundNodeKind.IsNullExpression:
                    VerifyIsNullExpression((BoundIsNullExpression)node, available, context);
                    break;
                case BoundNodeKind.CaseExpression:
                    VerifyCaseExpression((BoundCaseExpression)node, available, context);
                    break;
                case BoundNodeKind.SingleRowSubselect:
                    VerifySingleRowSubselect((BoundSingleRowSubselect)node, available, context);
                    break;
                case BoundNodeKind.ExistsSubselect:
                    VerifyExistsSubselect((BoundExistsSubselect)node, available, context);
                    break;
                case BoundNodeKind.ErrorExpression:
                    VerifyErrorExpression((BoundErrorExpression)node, available, context);
                    break;
                case BoundNodeKind.TableExpression:
                    VerifyTableExpression((BoundTableExpression)node, available, context);
                    break;
                case BoundNodeKind.ColumnExpression:
                    VerifyColumnExpression((BoundColumnExpression)node, available, context);
                    break;
                case BoundNodeKind.LiteralExpression:
                    VerifyLiteralExpression((BoundLiteralExpression)node, available, context);
                    break;
                case BoundNodeKind.VariableExpression:
                    VerifyVariableExpression((BoundVariableExpression)node, available, context);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private void VerifyValueSlotExpression(BoundValueSlotExpression node, FrozenSet<ValueSlot> available, string context)
        {
            CheckReference(node.ValueSlot, available, context);
        }

        private void VerifyUnaryExpression(BoundUnaryExpression node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyExpression(node.Expression, available, context);
        }

        private void VerifyBinaryExpression(BoundBinaryExpression node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyExpression(node.Left, available, context);
            VerifyExpression(node.Right, available, context);
        }

        private void VerifyFunctionInvocationExpression(BoundFunctionInvocationExpression node, FrozenSet<ValueSlot> available, string context)
        {
            foreach (var argument in node.Arguments)
                VerifyExpression(argument, available, context);
        }

        private void VerifyAggregateExpression(BoundAggregateExpression node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyExpression(node.Argument, available, context);
        }

        private void VerifyPropertyAccessExpression(BoundPropertyAccessExpression node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyExpression(node.Target, available, context);
        }

        private void VerifyMethodInvocationExpression(BoundMethodInvocationExpression node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyExpression(node.Target, available, context);
            foreach (var argument in node.Arguments)
                VerifyExpression(argument, available, context);
        }

        private void VerifyConversionExpression(BoundConversionExpression node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyExpression(node.Expression, available, context);
        }

        private void VerifyIsNullExpression(BoundIsNullExpression node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyExpression(node.Expression, available, context);
        }

        private void VerifyCaseExpression(BoundCaseExpression node, FrozenSet<ValueSlot> available, string context)
        {
            foreach (var caseLabel in node.CaseLabels)
            {
                VerifyExpression(caseLabel.Condition, available, context);
                VerifyExpression(caseLabel.ThenExpression, available, context);
            }

            VerifyExpression(node.ElseExpression, available, context);
        }

        private void VerifySingleRowSubselect(BoundSingleRowSubselect node, FrozenSet<ValueSlot> available, string context)
        {
            // A subselect may correlate to everything visible at its use site.
            VerifyRelation(node.Relation, available);
            CheckReference(node.Value, Defined(node.Relation), "a single-row subselect result");
        }

        private void VerifyExistsSubselect(BoundExistsSubselect node, FrozenSet<ValueSlot> available, string context)
        {
            VerifyRelation(node.Relation, available);
        }

        // Leaf expressions: no value-slot references.

        private void VerifyErrorExpression(BoundErrorExpression node, FrozenSet<ValueSlot> available, string context)
        {
        }

        private void VerifyTableExpression(BoundTableExpression node, FrozenSet<ValueSlot> available, string context)
        {
        }

        private void VerifyColumnExpression(BoundColumnExpression node, FrozenSet<ValueSlot> available, string context)
        {
        }

        private void VerifyLiteralExpression(BoundLiteralExpression node, FrozenSet<ValueSlot> available, string context)
        {
        }

        private void VerifyVariableExpression(BoundVariableExpression node, FrozenSet<ValueSlot> available, string context)
        {
        }

        // Shared helpers (not part of the visitor dispatch).

        private void VerifyAggregation(BoundRelation input, ImmutableArray<BoundComparedValue> groups, ImmutableArray<BoundAggregatedValue> aggregates, FrozenSet<ValueSlot> outer)
        {
            VerifyRelation(input, outer);

            var available = Available(outer, input);
            foreach (var group in groups)
                CheckReference(group.ValueSlot, available, "a group-by key");

            foreach (var aggregate in aggregates)
                VerifyExpression(aggregate.Argument, available, "an aggregate argument");
        }

        private void VerifyUnified(ImmutableArray<BoundRelation> inputs, IEnumerable<BoundUnifiedValue> definedValues, FrozenSet<ValueSlot> outer)
        {
            foreach (var input in inputs)
                VerifyRelation(input, outer);

            foreach (var unifiedValue in definedValues)
            {
                if (unifiedValue.InputValueSlots.Length != inputs.Length)
                {
                    Report($"Unified value '{unifiedValue.ValueSlot.Name}' has {unifiedValue.InputValueSlots.Length} inputs but the relation has {inputs.Length}.");
                    continue;
                }

                for (var i = 0; i < inputs.Length; i++)
                    CheckReference(unifiedValue.InputValueSlots[i], Available(outer, inputs[i]), $"a unified value over input {i}");
            }
        }

        private void VerifyArityMatch(BoundRelation left, BoundRelation right, string context)
        {
            var leftCount = left.GetOutputValues().Count();
            var rightCount = right.GetOutputValues().Count();
            if (leftCount != rightCount)
                Report($"Inputs to {context} have mismatched arity ({leftCount} vs {rightCount}).");
        }

        private void CheckReference(ValueSlot slot, FrozenSet<ValueSlot> available, string context)
        {
            if (slot is null)
                return;

            if (available.Contains(slot))
                return;

            Report($"Value slot '{slot.Name}' referenced by {context} is not defined in scope.");
        }

        private void Report(string problem)
        {
            _problems.Add(problem);
        }

        private void ThrowIfFailed()
        {
            if (_problems.Count > 0)
                throw new BoundTreeVerificationException(_problems);
        }

        // The slots visible to a node's own clauses: what its inputs expose, plus
        // whatever the enclosing scope makes available for correlation.
        private static FrozenSet<ValueSlot> Available(FrozenSet<ValueSlot> outer, params BoundRelation[] inputs)
        {
            var result = new HashSet<ValueSlot>(outer);
            foreach (var input in inputs)
                result.UnionWith(input.GetDefinedValues());
            return result.ToFrozenSet();
        }

        private static FrozenSet<ValueSlot> Defined(BoundRelation relation)
        {
            return relation.GetDefinedValues().ToFrozenSet();
        }
    }
}
