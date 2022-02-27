using NQuery.Binding;
using NQuery.Symbols.Aggregation;

namespace NQuery.Optimization
{
    internal sealed class SubqueryExpander : BoundTreeRewriter
    {
        private readonly Stack<List<Subquery>> _subqueryStack = new Stack<List<Subquery>>();
        private readonly Stack<BoundExpression> _passthruStack = new Stack<BoundExpression>();

        private enum SubqueryKind
        {
            Exists,
            Subselect
        }

        private sealed class Subquery
        {
            public Subquery(SubqueryKind kind, ValueSlot valueSlot, BoundRelation relation, BoundExpression passthru)
            {
                Kind = kind;
                ValueSlot = valueSlot;
                Relation = relation;
                Passthru = passthru;
            }

            public SubqueryKind Kind { get; }
            public ValueSlot ValueSlot { get; }
            public BoundRelation Relation { get; }
            public BoundExpression Passthru { get; }
        }

        public BoundExpression CurrentPassthru
        {
            get { return _passthruStack.Count == 0 ? null : _passthruStack.Peek(); }
        }

        public override BoundRelation RewriteRelation(BoundRelation node)
        {
            _subqueryStack.Push(new List<Subquery>());

            var result = base.RewriteRelation(node);

            if (_subqueryStack.Peek().Any())
                throw ExceptionBuilder.UnexpectedValue(node.Kind);

            _subqueryStack.Pop();

            return result;
        }

        protected override BoundExpression RewriteCaseExpression(BoundCaseExpression node)
        {
            var whenPassthru = CurrentPassthru;

            var labels = node.CaseLabels;
            foreach (var oldLabel in labels)
            {
                _passthruStack.Push(whenPassthru);
                var when = RewriteExpression(oldLabel.Condition);
                _passthruStack.Pop();

                var thenPassthru = Expression.Or(whenPassthru, Expression.Not(when));

                _passthruStack.Push(thenPassthru);
                var then = RewriteExpression(oldLabel.ThenExpression);
                _passthruStack.Pop();

                var label = oldLabel.Update(when, then);

                labels = labels.Replace(oldLabel, label);

                whenPassthru = Expression.Or(whenPassthru, when);
            }

            _passthruStack.Push(whenPassthru);
            var elseExpression = RewriteExpression(node.ElseExpression);
            _passthruStack.Pop();

            return node.Update(labels, elseExpression);
        }

        protected override BoundExpression RewriteSingleRowSubselect(BoundSingleRowSubselect node)
        {
            var relation = RewriteRelation(node.Relation);

            var factory = node.Value.Factory;

            // TODO: If the query is guaranteed to return a single, e.g. if it is a aggregated but not grouped,
            //       we should not emit the additional aggregation and assertion.

            // 1. We need to know whether it returns more than one row.

            var valueSlot = node.Value;
            var anyOutput = factory.CreateTemporary(valueSlot.Type);
            var countOutput = factory.CreateTemporary(typeof(int));

            var anyAggregateSymbol = BuiltInAggregates.Any;
            var anyAggregatable = anyAggregateSymbol.Definition.CreateAggregatable(valueSlot.Type);

            var countAggregatedSymbol = BuiltInAggregates.Count;
            var countAggregatable = countAggregatedSymbol.Definition.CreateAggregatable(typeof(int));

            var aggregates = new[]
            {
                new BoundAggregatedValue(anyOutput, anyAggregateSymbol, anyAggregatable, Expression.Value(valueSlot)),
                new BoundAggregatedValue(countOutput, countAggregatedSymbol, countAggregatable, Expression.Literal(0))
            };

            var aggregation = new BoundGroupByAndAggregationRelation(relation, Enumerable.Empty<BoundComparedValue>(), aggregates);

            // 2. Now we can assert that the number of rows returned is at most one.

            var condition = Expression.LessThan(Expression.Value(countOutput), Expression.Literal(1));

            var message = Resources.SubqueryReturnedMoreThanRow;
            var assertRelation = new BoundAssertRelation(aggregation, condition, message);

            var subquery = new Subquery(SubqueryKind.Subselect, anyOutput, assertRelation, CurrentPassthru);
            var subqueries = _subqueryStack.Peek();
            subqueries.Add(subquery);

            return Expression.Value(anyOutput);
        }

        protected override BoundExpression RewriteExistsSubselect(BoundExistsSubselect node)
        {
            var relation = RewriteRelation(node.Relation);

            // TODO: This isn't ideal. In many cases, the relation will not have any output values.
            //       This means, we've to create a new value slot factory which in turn means that
            //       we have to create multiple slots with the same name. It seems we should think of
            //       a better way to carry the factories. Maybe we should just expose on specific
            //       bound nodes?

            var factory = node.Relation.GetOutputValues().FirstOrDefault()?.Factory ?? new ValueSlotFactory();

            var valueSlot = factory.CreateTemporary(typeof(bool));
            var subquery = new Subquery(SubqueryKind.Exists, valueSlot, relation, CurrentPassthru);
            var subqueries = _subqueryStack.Peek();
            subqueries.Add(subquery);

            return Expression.Value(valueSlot);
        }

        protected override BoundRelation RewriteFilterRelation(BoundFilterRelation node)
        {
            var input = RewriteRelation(node.Input);

            var rewrittenRelation = RewriteConjunctions(input, node.Condition);
            if (rewrittenRelation is not null)
            {
                // We might have residual subqueries that couldn't be
                // converted to semi joins.
                return RewriteRelation(rewrittenRelation);
            }

            // There were no subqueries that could be expressed as semi joins.
            // However, there might still exist subqueries, so we need to visit
            // the expression that will convert them to probing semi joins.

            var condition = RewriteExpression(node.Condition);
            var inputWithProbes = RewriteInputWithSubqueries(input);
            return node.Update(inputWithProbes, condition);
        }

        protected override BoundRelation RewriteComputeRelation(BoundComputeRelation node)
        {
            var input = RewriteRelation(node.Input);
            var computedValues = RewriteComputedValues(node.DefinedValues);
            var inputWithProbes = RewriteInputWithSubqueries(input);
            return node.Update(inputWithProbes, computedValues);
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            var left = RewriteRelation(node.Left);
            var right = RewriteRelation(node.Right);

            var rewrittenRight = RewriteConjunctions(right, node.Condition);
            if (rewrittenRight is not null)
            {
                // We might have residual subqueries that couldn't be
                // converted to semi joins.
                return RewriteRelation(node.Update(node.JoinType, left, rewrittenRight, null, node.Probe, node.PassthruPredicate));
            }

            // There were no subqueries that could be expressed as semi joins.
            // However, there might still exist subqueries, so we need to visit
            // the expression that will convert them to probing semi joins.

            var condition = RewriteExpression(node.Condition);
            var rightWithProbes = RewriteInputWithSubqueries(right);
            return node.Update(node.JoinType, left, rightWithProbes, condition, node.Probe, node.PassthruPredicate);
        }

        private BoundRelation RewriteInputWithSubqueries(BoundRelation input)
        {
            var subqueries = _subqueryStack.Peek();
            var result = input;

            result = subqueries.Where(s => s.Kind == SubqueryKind.Exists)
                               .Aggregate(result, EmitExists);

            result = subqueries.Where(s => s.Kind == SubqueryKind.Subselect)
                               .Aggregate(result, EmitSubselect);

            subqueries.Clear();

            return result;
        }

        private static BoundJoinRelation EmitExists(BoundRelation result, Subquery existsSubquery)
        {
            return new BoundJoinRelation(BoundJoinType.LeftSemi, result, existsSubquery.Relation, null, existsSubquery.ValueSlot, existsSubquery.Passthru);
        }

        private static BoundJoinRelation EmitSubselect(BoundRelation result, Subquery subselect)
        {
            return new BoundJoinRelation(BoundJoinType.LeftOuter, result, subselect.Relation, null, null, subselect.Passthru);
        }

        private static BoundRelation RewriteConjunctions(BoundRelation input, BoundExpression condition)
        {
            var current = input;
            var scalarPredicates = new List<BoundExpression>();
            var conjunctions = Expression.SplitConjunctions(condition);

            foreach (var conjunction in conjunctions)
            {
                BoundExistsSubselect exists;
                bool isNegated;
                if (TryGetExistsSubselect(conjunction, out exists, out isNegated))
                {
                    var joinType = isNegated
                        ? BoundJoinType.LeftAntiSemi
                        : BoundJoinType.LeftSemi;
                    current = new BoundJoinRelation(joinType, current, exists.Relation, null, null, null);
                }
                else if (Expression.IsDisjunction(conjunction))
                {
                    var relation = RewriteDisjunctions(conjunction);
                    if (relation is not null)
                        current = new BoundJoinRelation(BoundJoinType.LeftSemi, current, relation, null, null, null);
                    else
                        scalarPredicates.Add(conjunction);
                }
                else
                {
                    scalarPredicates.Add(conjunction);
                }
            }

            // If we haven't done anything, simply return null to indicate to our
            // caller that the condition only contained scalars.

            if (current == input)
                return null;

            // If we have no scalar predicates left, it means the condition only
            // contained EXISTs queries, so we can return the current node.

            if (scalarPredicates.Count == 0)
                return current;

            // Otherwise we add a filter for the scalars.

            var predicate = Expression.And(scalarPredicates);
            return new BoundFilterRelation(current, predicate);
        }

        private static BoundRelation RewriteDisjunctions(BoundExpression condition)
        {
            var scalarPredicates = new List<BoundExpression>();
            var relationalPredicates = new List<BoundRelation>();

            foreach (var disjunction in Expression.SplitDisjunctions(condition))
            {
                BoundExistsSubselect exists;
                bool isNegated;
                if (TryGetExistsSubselect(disjunction, out exists, out isNegated))
                {
                    if (!isNegated)
                    {
                        relationalPredicates.Add(exists.Relation);
                    }
                    else
                    {
                        var constantRelation = new BoundConstantRelation();
                        var predicate = new BoundJoinRelation(BoundJoinType.LeftAntiSemi, constantRelation, exists.Relation, null, null, null);
                        relationalPredicates.Add(predicate);
                    }
                }
                else if (Expression.IsConjunction(disjunction))
                {
                    var constantRelation = new BoundConstantRelation();
                    var output = RewriteConjunctions(constantRelation, disjunction);
                    if (output is null)
                    {
                        scalarPredicates.Add(disjunction);
                    }
                    else
                    {
                        var predicate = new BoundJoinRelation(BoundJoinType.LeftSemi, constantRelation, output, null, null, null);
                        relationalPredicates.Add(predicate);
                    }
                }
                else
                {
                    scalarPredicates.Add(disjunction);
                }
            }

            if (relationalPredicates.Count == 0)
                return null;

            if (scalarPredicates.Count > 0)
            {
                var constantRelation = new BoundConstantRelation();
                var predicate = Expression.Or(scalarPredicates);
                var filter = new BoundFilterRelation(constantRelation, predicate);
                relationalPredicates.Insert(0, filter);
            }

            return new BoundConcatenationRelation(relationalPredicates, Enumerable.Empty<BoundUnifiedValue>());
        }

        private static bool TryGetExistsSubselect(BoundExpression expression, out BoundExistsSubselect existsSubselect, out bool isNegated)
        {
            var negation = expression as BoundUnaryExpression;
            if (negation is not null && negation.Result.Selected.Signature.Kind == UnaryOperatorKind.LogicalNot)
            {
                if (!TryGetExistsSubselect(negation.Expression, out existsSubselect, out isNegated))
                    return false;

                isNegated = !isNegated;
                return true;
            }

            isNegated = false;
            existsSubselect = expression as BoundExistsSubselect;
            return existsSubselect is not null;
        }
    }
}