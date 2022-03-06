using System.Collections.Immutable;
using System.Diagnostics;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Optimization
{
    internal sealed class CommonTableExpressionInstantiator : BoundTreeRewriter
    {
        private readonly CommonTableExpressionSymbol _symbol;

        public CommonTableExpressionInstantiator()
        {
        }

        private CommonTableExpressionInstantiator(CommonTableExpressionSymbol symbol)
        {
            _symbol = symbol;
        }

        protected override BoundRelation RewriteTableRelation(BoundTableRelation node)
        {
            if (node.TableInstance.Table is not CommonTableExpressionSymbol symbol)
                return base.RewriteTableRelation(node);

            if (IsInstantiatedCommonTableExpression(node))
            {
                return new BoundConstantRelation();
            }

            var outputValues = node.GetOutputValues().ToImmutableArray();
            var instantiatedQuery = symbol.RecursiveMembers.Any()
                ? InstantiateRecursiveCommonTableExpression(outputValues, symbol)
                : InstantiateCommonTableExpression(outputValues, symbol);

            // For regular SELECT queries the output node will be a projection.
            // We don't need this moving forward so it's safe to omit.
            //
            // NOTE: We need to keep this after we added the output slot mapping.
            //       Otherwise the slots will not align.

            var result = instantiatedQuery is not BoundProjectRelation projection ? instantiatedQuery : projection.Input;

            return result;
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            BoundRelation otherSide;
            if (IsInstantiatedCommonTableExpression(node.Left))
            {
                otherSide = RewriteRelation(node.Right);
            }
            else if (IsInstantiatedCommonTableExpression(node.Right))
            {
                otherSide = RewriteRelation(node.Left);
            }
            else
            {
                return base.RewriteJoinRelation(node);
            }

            Debug.Assert(node.JoinType == BoundJoinType.Inner);

            return node.Condition is null
                ? otherSide
                : new BoundFilterRelation(otherSide, node.Condition);
        }

        private bool IsInstantiatedCommonTableExpression(BoundRelation boundRelation)
        {
            if (_symbol is null)
                return false;

            if (boundRelation is not BoundTableRelation tableRelation)
                return false;

            return tableRelation.TableInstance.Table == _symbol;
        }

        private BoundRelation InstantiateCommonTableExpression(ImmutableArray<ValueSlot> outputValues, CommonTableExpressionSymbol symbol)
        {
            var prototype = symbol.Anchor.Relation;
            var prototypeOutputValues = prototype.GetOutputValues();
            var mapping = outputValues.Zip(prototypeOutputValues, (v, k) => new KeyValuePair<ValueSlot, ValueSlot>(k, v));

            var instantiatedRelation = Instantiator.Instantiate(prototype, mapping);
            return RewriteRelation(instantiatedRelation);
        }

        private BoundRelation InstantiateRecursiveCommonTableExpression(ImmutableArray<ValueSlot> outputValues, CommonTableExpressionSymbol symbol)
        {
            // TableSpoolPusher
            //     Concat
            //         Compute (Recursion := 0)
            //             <Anchor>
            //         Assert (Recursion <= 100)
            //             Inner Join
            //                 Compute (Recursion := Recursion + 1)
            //                     TableSpoolPopper
            //                 Concat
            //                      Filter <RecursiveJoinPredicate1>
            //                          <RecursiveMember1>
            //                      Filter <RecursiveJoinPredicate2>
            //                          <RecursiveMember2>

            var valueSlotFactory = outputValues.First().Factory;

            // Create output values

            var unionRecursionSlot = valueSlotFactory.CreateTemporary(typeof(int));
            var concatValueSlots = outputValues.Add(unionRecursionSlot);

            // Create anchor

            var anchor = RewriteRelation(Instantiator.Instantiate(symbol.Anchor.Relation));
            var anchorValues = anchor.GetOutputValues().ToImmutableArray();

            var initRecursionSlot = valueSlotFactory.CreateTemporary(typeof(int));
            var initRecursionDefinition = new BoundComputedValue(Expression.Literal(0), initRecursionSlot);
            var initRecursion = new BoundComputeRelation(anchor, ImmutableArray.Create(initRecursionDefinition));

            var initRecursionOutputs = anchorValues.Add(initRecursionSlot);

            // Create TableSpoolPopper

            var tableSpoolPopperSlots = initRecursionOutputs.Select(v => v.Duplicate()).ToImmutableArray();

            var tableSpoolPopper = new BoundTableSpoolPopper(tableSpoolPopperSlots);

            var anchorRecursionCounter = tableSpoolPopperSlots.Last();
            var inc = Expression.Plus(Expression.Value(anchorRecursionCounter), Expression.Literal(1));
            var incRecursionSlot = valueSlotFactory.CreateTemporary(typeof(int));
            var incRecursionDefinition = new BoundComputedValue(inc, incRecursionSlot);
            var incRecursion = new BoundComputeRelation(tableSpoolPopper, ImmutableArray.Create(incRecursionDefinition));

            // Create recursive members

            var recursiveRewriter = new CommonTableExpressionInstantiator(symbol);
            var recursiveMembers = new List<BoundRelation>(symbol.RecursiveMembers.Length);
            var recursiveMemberOutputs = new List<ImmutableArray<ValueSlot>>(symbol.RecursiveMembers.Length);

            var anchorReferences = tableSpoolPopperSlots.RemoveAt(tableSpoolPopperSlots.Length - 1);

            foreach (var recursiveMember in symbol.RecursiveMembers)
            {
                var recursivePrototype = recursiveMember.Relation;
                var mapping = CreateRecursiveMemberInstanceValueSlotMapping(symbol, anchorReferences, recursivePrototype);
                var recursiveInstance = Instantiator.Instantiate(recursivePrototype, mapping);
                var recursiveRelation = recursiveRewriter.RewriteRelation(recursiveInstance);
                var recursiveRelationOutputs = recursiveRelation.GetOutputValues().ToImmutableArray();
                recursiveMembers.Add(recursiveRelation);
                recursiveMemberOutputs.Add(recursiveRelationOutputs);
            }

            // Concatenate recursive members

            var recursiveConcatValues = Enumerable
                .Range(0, concatValueSlots.Length - 1)
                .Select(i =>
                {
                    var slot = valueSlotFactory.CreateTemporary(concatValueSlots[i].Type);
                    return new BoundUnifiedValue(slot, recursiveMemberOutputs.Select(o => o[i]));
                })
                .ToImmutableArray();

            var hasSingleRecursiveMember = recursiveMembers.Count == 1;

            var recursiveConcat = hasSingleRecursiveMember ? recursiveMembers.Single() : new BoundConcatenationRelation(recursiveMembers, recursiveConcatValues);
            var recursionOutputs = hasSingleRecursiveMember ? recursiveMemberOutputs.Single() : recursiveConcatValues.Select(u => u.ValueSlot).ToImmutableArray();

            // Create inner join

            var join = new BoundJoinRelation(BoundJoinType.Inner, incRecursion, recursiveConcat, null, null, null);
            var joinOutputs = recursionOutputs.Add(incRecursionSlot);
            var recursiveProjection = new BoundProjectRelation(join, joinOutputs);

            // Create assert

            var assertCondition = Expression.LessThan(Expression.Value(incRecursionSlot), Expression.Literal(100));
            var assert = new BoundAssertRelation(recursiveProjection, assertCondition, Resources.MaximumRecursionLevelExceeded);

            // Create top level concat

            var concatValues = concatValueSlots.Select((v, i) =>
            {
                var slots = new[]
                {
                    initRecursionOutputs[i],
                    joinOutputs[i]
                };

                return new BoundUnifiedValue(v, slots);
            });

            var concatInputs = new BoundRelation[] { initRecursion, assert };
            var concat = new BoundConcatenationRelation(concatInputs, concatValues);

            var tableSpoolPusher = new BoundTableSpoolPusher(concat);

            return new BoundProjectRelation(tableSpoolPusher, concatValueSlots.Take(concatValueSlots.Length - 1));
        }

        private static ImmutableDictionary<ValueSlot, ValueSlot> CreateRecursiveMemberInstanceValueSlotMapping(CommonTableExpressionSymbol symbol, ImmutableArray<ValueSlot> instanceSlots, BoundRelation relation)
        {
            var finder = new CommonTableExpressionInstanceFinder(symbol);
            finder.VisitRelation(relation);

            Debug.Assert(finder.Instance is not null && finder.Instance.Table == symbol);
            Debug.Assert(instanceSlots.Length == finder.Instance.ColumnInstances.Length);

            return instanceSlots.Zip(finder.Instance.ColumnInstances, (v, c) => new KeyValuePair<ValueSlot, ValueSlot>(c.ValueSlot, v))
                                .ToImmutableDictionary();
        }

        private sealed class CommonTableExpressionInstanceFinder : BoundTreeWalker
        {
            private readonly CommonTableExpressionSymbol _symbol;
            private TableInstanceSymbol _instance;

            public CommonTableExpressionInstanceFinder(CommonTableExpressionSymbol symbol)
            {
                _symbol = symbol;
            }

            public TableInstanceSymbol Instance
            {
                get { return _instance; }
            }

            protected override void VisitTableRelation(BoundTableRelation node)
            {
                if (node.TableInstance.Table == _symbol)
                {
                    Debug.Assert(_instance is null);
                    _instance = node.TableInstance;
                }
                base.VisitTableRelation(node);
            }
        }
    }
}