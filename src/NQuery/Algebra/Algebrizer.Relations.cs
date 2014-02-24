using System;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Algebra
{
    partial class Algebrizer
    {
        private static AlgebraQueryCombinator GetQueryCombinator(BoundQueryCombinator combinator)
        {
            switch (combinator)
            {
                case BoundQueryCombinator.Union:
                    return AlgebraQueryCombinator.Union;
                case BoundQueryCombinator.UnionAll:
                    return AlgebraQueryCombinator.UnionAll;
                case BoundQueryCombinator.Except:
                    return AlgebraQueryCombinator.Except;
                case BoundQueryCombinator.Intersect:
                    return AlgebraQueryCombinator.Intersect;
                default:
                    throw new ArgumentOutOfRangeException("combinator");
            }
        }

        private static AlgebraJoinKind GetJoinKind(BoundJoinType joinType)
        {
            switch (joinType)
            {
                case BoundJoinType.Inner:
                    return AlgebraJoinKind.Inner;
                case BoundJoinType.FullOuter:
                    return AlgebraJoinKind.FullOuter;
                case BoundJoinType.LeftOuter:
                    return AlgebraJoinKind.LeftOuter;
                case BoundJoinType.RightOuter:
                    return AlgebraJoinKind.RightOuter;
                default:
                    throw new ArgumentOutOfRangeException("joinType");
            }
        }

        private AlgebraRelation AlgebrizeTableRelation(BoundTableRelation node)
        {
            // TODO: If expressions.TableInstance refers to CTE, we need to instantiate the CTE here.

            var cte = node.TableInstance.Table as CommonTableExpressionSymbol;
            return cte == null
                       ? new AlgebraTableNode(node.TableInstance)
                       : AlgebrizeRelation(cte.Query);
        }

        private AlgebraRelation AlgebrizeDerivedTableRelation(BoundDerivedTableRelation node)
        {
            return AlgebrizeRelation(node.Query);
        }

        private AlgebraRelation AlgebrizeJoinRelation(BoundJoinRelation node)
        {
            var condition = node.Condition;
            var physicalJoin = GetJoinKind(node.JoinType);
            var algebrizedLeft = AlgebrizeRelation(node.Left);
            var algebrizedRight = AlgebrizeRelation(node.Right);
            var algebrizedCondition = condition == null
                                        ? null
                                        : AlgebrizeExpression(condition);
            return new AlgebraJoinNode(physicalJoin, algebrizedLeft, algebrizedRight, algebrizedCondition);
        }

        private AlgebraRelation AlgebrizeRelation(BoundRelation node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.CombinedRelation:
                    return AlgebrizeCombinedRelation((BoundCombinedRelation)node);
                case BoundNodeKind.TableRelation:
                    return AlgebrizeTableRelation((BoundTableRelation)node);
                case BoundNodeKind.DerivedTableRelation:
                    return AlgebrizeDerivedTableRelation((BoundDerivedTableRelation)node);
                case BoundNodeKind.JoinRelation:
                    return AlgebrizeJoinRelation((BoundJoinRelation) node);
                case BoundNodeKind.QueryRelation:
                    return AlgebrizeQueryRelation((BoundQueryRelation)node);
                case BoundNodeKind.ComputeRelation:
                    return AlgebrizeCompute((BoundComputeRelation)node);
                case BoundNodeKind.FilterRelation:
                    return AlgebrizeFilter((BoundFilterRelation)node);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return AlgebrizeGroupByAndAggregation((BoundGroupByAndAggregationRelation)node);
                case BoundNodeKind.ConstantRelation:
                    return AlgebrizeConstant((BoundConstantRelation)node);
                case BoundNodeKind.ProjectRelation:
                    return AlgebrizeProject((BoundProjectRelation)node);
                case BoundNodeKind.SortRelation:
                    return AlgebrizeSort((BoundSortRelation)node);
                case BoundNodeKind.TopRelation:
                    return AlgebrizeTop((BoundTopRelation)node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private AlgebraRelation AlgebrizeCombinedRelation(BoundCombinedRelation node)
        {
            var left = AlgebrizeRelation(node.Left);
            var right = AlgebrizeRelation(node.Right);
            var combinator = GetQueryCombinator(node.Combinator);
            return new AlgebraCombinedQuery(left, right, combinator, node.Outputs);
        }

        private AlgebraRelation AlgebrizeConstant(BoundConstantRelation node)
        {
            return new AlgebraConstantNode();
        }

        private AlgebraRelation AlgebrizeFilter(BoundFilterRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var condition = AlgebrizeExpression(node.Condition);
            return new AlgebraFilterNode(input, condition);
        }

        private AlgebraRelation AlgebrizeCompute(BoundComputeRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var definedValues = (from c in node.DefinedValues
                                 let expression = c.Expression
                                 let valueSlot = c.ValueSlot
                                 where !(expression is BoundValueSlotExpression)
                                 let algebraExpression = AlgebrizeExpression(expression)
                                 select new AlgebraComputedValue(algebraExpression, valueSlot)).ToArray();

            if (definedValues.Length == 0)
                return input;

            return new AlgebraComputeNode(input, definedValues);
        }

        private AlgebraRelation AlgebrizeGroupByAndAggregation(BoundGroupByAndAggregationRelation node)
        {
            var input = AlgebrizeRelation(node.Input);

            // TODO: Emit compute for GROUPs

            var columns = node.Groups;
            var algberizedAggregates = (from t in node.Aggregates
                                        let aggregate = t.Aggregate
                                        let argument = AlgebrizeExpression(t.Argument)
                                        let output = t.Output
                                        select new AlgebraAggregateDefinition(output, aggregate, argument)).ToArray();

            return new AlgebraGroupByAndAggregation(input, columns, algberizedAggregates);
        }

        private AlgebraRelation AlgebrizeSort(BoundSortRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var expressions = node.SortedValues.Select(c => c.ValueSlot).ToArray();
            var comparers = node.SortedValues.Select(c => c.Comparer).ToArray();
            return new AlgebraSortNode(input, expressions, comparers);
        }

        private AlgebraRelation AlgebrizeTop(BoundTopRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var tieEntries = node.TieEntries.Select(c => c.ValueSlot).ToArray();
            var tieComparer = node.TieEntries.Select(c => c.Comparer).ToArray();
            return new AlgebraTopNode(input, node.Limit, tieEntries, tieComparer);
        }

        private AlgebraRelation AlgebrizeProject(BoundProjectRelation node)
        {
            var input = AlgebrizeRelation(node.Input);
            var output = node.Outputs;
            return new AlgebraProjectNode(input, output);
        }

        private AlgebraRelation AlgebrizeQueryRelation(BoundQueryRelation node)
        {
            return AlgebrizeRelation(node.Relation);
        }
    }
}