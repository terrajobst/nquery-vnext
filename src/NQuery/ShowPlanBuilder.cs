using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Binding;

namespace NQuery
{
    internal static class ShowPlanBuilder
    {
        public static ShowPlan Build(string name, BoundQuery node)
        {
            var showPlanNode = Build(node);
            return new ShowPlan(name, showPlanNode);
        }

        private static ShowPlanNode Build(BoundNode node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.Query:
                    return BuildQueryRelation((BoundQuery)node);
                case BoundNodeKind.ConstantRelation:
                    return BuildConstant((BoundConstantRelation)node);
                case BoundNodeKind.TableRelation:
                    return BuildTable((BoundTableRelation)node);
                case BoundNodeKind.DerivedTableRelation:
                    return BuildDerivedTable((BoundDerivedTableRelation)node);
                case BoundNodeKind.FilterRelation:
                    return BuildFilter((BoundFilterRelation)node);
                case BoundNodeKind.ComputeRelation:
                    return BuildCompute((BoundComputeRelation)node);
                case BoundNodeKind.JoinRelation:
                    return BuildJoin((BoundJoinRelation)node);
                case BoundNodeKind.HashMatchRelation:
                    return BuildHashMatch((BoundHashMatchRelation)node);
                case BoundNodeKind.TopRelation:
                    return BuildTop((BoundTopRelation)node);
                case BoundNodeKind.SortRelation:
                    return BuildSort((BoundSortRelation)node);
                case BoundNodeKind.CombinedRelation:
                    return BuildCombinedQuery((BoundCombinedRelation)node);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return BuildGroupByAndAggregation((BoundGroupByAndAggregationRelation)node);
                case BoundNodeKind.StreamAggregatesRelation:
                    return BuildStreamAggregatesRelation((BoundStreamAggregatesRelation)node);
                case BoundNodeKind.ProjectRelation:
                    return BuildProject((BoundProjectRelation)node);
                case BoundNodeKind.UnaryExpression:
                    return BuildUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return BuildBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.LiteralExpression:
                    return BuildLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.ValueSlotExpression:
                    return BuildValueSlotExpression((BoundValueSlotExpression)node);
                case BoundNodeKind.VariableExpression:
                    return BuildVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.FunctionInvocationExpression:
                    return BuildFunctionInvocationExpression((BoundFunctionInvocationExpression)node);
                case BoundNodeKind.PropertyAccessExpression:
                    return BuildPropertyAccessExpression((BoundPropertyAccessExpression)node);
                case BoundNodeKind.MethodInvocationExpression:
                    return BuildMethodInvocationExpression((BoundMethodInvocationExpression)node);
                case BoundNodeKind.ConversionExpression:
                    return BuildConversionExpression((BoundConversionExpression)node);
                case BoundNodeKind.IsNullExpression:
                    return BuildIsNullExpression((BoundIsNullExpression)node);
                case BoundNodeKind.CaseExpression:
                    return BuildCaseExpression((BoundCaseExpression)node);
                case BoundNodeKind.SingleRowSubselect:
                    return BuildSingleRowSubselect((BoundSingleRowSubselect)node);
                case BoundNodeKind.ExistsSubselect:
                    return BuildExistsSubselect((BoundExistsSubselect)node);
                case BoundNodeKind.AllAnySubselect:
                    return BuildAllAnySubselect((BoundAllAnySubselect)node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ShowPlanNode BuildQueryRelation(BoundQuery node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation) };
            var slots = string.Join(", ", node.OutputColumns.Select(v => v.Name));
            return new ShowPlanNode("Query " + slots, properties, children);
        }

        private static ShowPlanNode BuildConstant(BoundConstantRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode("Constant", properties, children);
        }

        private static ShowPlanNode BuildTable(BoundTableRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode("Table (" + node.TableInstance + ")", properties, children);
        }

        private static ShowPlanNode BuildDerivedTable(BoundDerivedTableRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation) };
            return new ShowPlanNode("Derived Table (" + node.TableInstance + ")", properties, children);
        }

        private static ShowPlanNode BuildFilter(BoundFilterRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input), Build(node.Condition) };
            return new ShowPlanNode("Filter", properties, children);
        }

        private static ShowPlanNode BuildCompute(BoundComputeRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var input = new[] {Build(node.Input)};
            var aggregates = from d in node.DefinedValues
                             let dName = d.ValueSlot.Name
                             let dProperties = Enumerable.Empty<KeyValuePair<string, string>>()
                             let dChildren = new[] { Build(d.Expression) }
                             select new ShowPlanNode(dName, dProperties, dChildren);
            var children = input.Concat(aggregates);
            return new ShowPlanNode("Compute", properties, children);
        }

        private static ShowPlanNode BuildJoin(BoundJoinRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var leftAndRight = new[]
                               {
                                   Build(node.Left),
                                   Build(node.Right)
                               };

            var children = node.Condition == null
                               ? leftAndRight
                               : leftAndRight.Concat(new[] {Build(node.Condition)});

            return new ShowPlanNode(node.JoinType + "Join", properties, children);
        }

        private static ShowPlanNode BuildHashMatch(BoundHashMatchRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var leftAndRight = new[]
                               {
                                   Build(node.Build),
                                   Build(node.Probe)
                               };

            var children = node.Remainder == null
                               ? leftAndRight
                               : leftAndRight.Concat(new[] { Build(node.Remainder) });

            return new ShowPlanNode(string.Format("Hash Match ({0}) [{1} = {2}]", node.LogicalOperator, node.BuildKey, node.ProbeKey), properties, children);
        }

        private static ShowPlanNode BuildTop(BoundTopRelation node)
        {
            var tieEntries = string.Join(", ", node.TieEntries.Select(v => v.ValueSlot.Name));
            var operatorName = string.Format("Top {0}{1}", node.Limit, !node.TieEntries.Any() ? string.Empty : string.Format(" With Ties ({0})", tieEntries));
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode(operatorName, properties, children);
        }

        private static ShowPlanNode BuildSort(BoundSortRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            var slots = string.Join(", ", node.SortedValues.Select(v => v.ValueSlot.Name));
            var op = node.IsDistinct ? "DistinctSort" : "Sort";
            var name = string.Format("{0} {1}", op, slots);
            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildCombinedQuery(BoundCombinedRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Right) };
            var slots = string.Join(", ", node.Outputs.Select(v => v.Name));
            var name = string.Format("{0} {1}", node.Combinator, slots);
            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildGroupByAndAggregation(BoundGroupByAndAggregationRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var input = new[] { Build(node.Input) };
            var aggregates = from a in node.Aggregates
                             let aName = a.Output + " = " + a.Aggregate.Name
                             let aProperties = Enumerable.Empty<KeyValuePair<string, string>>()
                             let aChildren = new[] {Build(a.Argument)}
                             select new ShowPlanNode(aName, aProperties, aChildren);
            var children = input.Concat(aggregates);
            var slots = string.Join(", ", node.Groups.Select(v => v.Name));
            return new ShowPlanNode("GroupByAndAggregation " + slots, properties, children);
        }

        private static ShowPlanNode BuildStreamAggregatesRelation(BoundStreamAggregatesRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var input = new[] { Build(node.Input) };
            var aggregates = from a in node.Aggregates
                             let aName = a.Output + " = " + a.Aggregate.Name
                             let aProperties = Enumerable.Empty<KeyValuePair<string, string>>()
                             let aChildren = new[] { Build(a.Argument) }
                             select new ShowPlanNode(aName, aProperties, aChildren);
            var children = input.Concat(aggregates);
            var slots = string.Join(", ", node.Groups.Select(v => v.Name));
            return new ShowPlanNode("StreamAggregates " + slots, properties, children);
        }

        private static ShowPlanNode BuildProject(BoundProjectRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            var slots = string.Join(", ", node.Outputs.Select(v => v.Name));
            return new ShowPlanNode("Project " + slots, properties, children);
        }

        private static ShowPlanNode BuildUnaryExpression(BoundUnaryExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] {Build(node.Expression)};
            return new ShowPlanNode(node.Result.Selected.Signature.Kind.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildBinaryExpression(BoundBinaryExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Right) };
            return new ShowPlanNode(node.Result.Selected.Signature.Kind.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildLiteralExpression(BoundLiteralExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            var value = node.Value == null
                            ? "NULL"
                            : node.Value is string
                                  ? "'" + node.Value.ToString().Replace("'", "''") + "'"
                                  : node.Value.ToString();
            return new ShowPlanNode("Literal (" + value + ")", properties, children, true);
        }

        private static ShowPlanNode BuildValueSlotExpression(BoundValueSlotExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode(node.ValueSlot.Name, properties, children, true);
        }

        private static ShowPlanNode BuildVariableExpression(BoundVariableExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildFunctionInvocationExpression(BoundFunctionInvocationExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = node.Arguments.Select(Build);
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildPropertyAccessExpression(BoundPropertyAccessExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Target) };
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildMethodInvocationExpression(BoundMethodInvocationExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] {Build(node.Target)}.Concat(node.Arguments.Select(Build));
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildConversionExpression(BoundConversionExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Expression) };
            return new ShowPlanNode("Conversion (" + node.Type.ToDisplayName() + ")", properties, children, true);
        }

        private static ShowPlanNode BuildIsNullExpression(BoundIsNullExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Expression) };
            return new ShowPlanNode("IsNull", properties, children, true);
        }

        private static ShowPlanNode BuildCaseExpression(BoundCaseExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var caseLabels = node.CaseLabels.Select(BuildCaseLabel);
            var elseLabel = node.ElseExpression == null
                                ? Enumerable.Empty<ShowPlanNode>()
                                : new[] {Build(node.ElseExpression)};
            var children = caseLabels.Concat(elseLabel);
            return new ShowPlanNode("Case", properties, children, true);
        }

        private static ShowPlanNode BuildCaseLabel(BoundCaseLabel node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[]
                               {
                                   Build(node.Condition),
                                   Build(node.ThenExpression)
                               };
            return new ShowPlanNode("When", properties, children, true);
        }

        private static ShowPlanNode BuildSingleRowSubselect(BoundSingleRowSubselect node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation)};
            return new ShowPlanNode("Subselect", properties, children, true);
        }

        private static ShowPlanNode BuildExistsSubselect(BoundExistsSubselect node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation) };
            return new ShowPlanNode("Exists", properties, children, true);
        }

        private static ShowPlanNode BuildAllAnySubselect(BoundAllAnySubselect node)
        {
            // TODO: Extract actual kind (ALL or ANY)
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Relation) };
            return new ShowPlanNode("AllAny (" + node.Result.Selected.Signature.Kind + ")", properties, children, true);
        }
    }
}