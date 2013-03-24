using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Algebra
{
    internal static class ShowPlanBuilder
    {
        public static ShowPlanNode Build(AlgebraNode node)
        {
            switch (node.Kind)
            {
                case AlgebraKind.Constant:
                    return BuildConstant((AlgebraConstantNode)node);
                case AlgebraKind.Table:
                    return BuildTable((AlgebraTableNode)node);
                case AlgebraKind.Filter:
                    return BuildFilter((AlgebraFilterNode)node);
                case AlgebraKind.Compute:
                    return BuildCompute((AlgebraComputeNode)node);
                case AlgebraKind.Join:
                    return BuildJoin((AlgebraJoinNode)node);
                case AlgebraKind.Top:
                    return BuildTop((AlgebraTopNode)node);
                case AlgebraKind.Sort:
                    return BuildSort((AlgebraSortNode)node);
                case AlgebraKind.BinaryQuery:
                    return BuildCombinedQuery((AlgebraCombinedQuery)node);
                case AlgebraKind.GroupByAndAggregation:
                    return BuildGroupByAndAggregation((AlgebraGroupByAndAggregation)node);
                case AlgebraKind.Project:
                    return BuildProject((AlgebraProjectNode)node);
                case AlgebraKind.UnaryExpression:
                    return BuildUnaryExpression((AlgebraUnaryExpression)node);
                case AlgebraKind.BinaryExpression:
                    return BuildBinaryExpression((AlgebraBinaryExpression)node);
                case AlgebraKind.LiteralExpression:
                    return BuildLiteralExpression((AlgebraLiteralExpression)node);
                case AlgebraKind.ValueSlotExpression:
                    return BuildValueSlotExpression((AlgebraValueSlotExpression)node);
                case AlgebraKind.VariableExpression:
                    return BuildVariableExpression((AlgebraVariableExpression)node);
                case AlgebraKind.FunctionInvocationExpression:
                    return BuildFunctionInvocationExpression((AlgebraFunctionInvocationExpression)node);
                case AlgebraKind.PropertyAccessExpression:
                    return BuildPropertyAccessExpression((AlgebraPropertyAccessExpression)node);
                case AlgebraKind.MethodInvocationExpression:
                    return BuildMethodInvocationExpression((AlgebraMethodInvocationExpression)node);
                case AlgebraKind.ConversionExpression:
                    return BuildConversionExpression((AlgebraConversionExpression)node);
                case AlgebraKind.IsNullExpression:
                    return BuildIsNullExpression((AlgebraIsNullExpression)node);
                case AlgebraKind.CaseExpression:
                    return BuildCaseExpression((AlgebraCaseExpression)node);
                case AlgebraKind.SingleRowSubselect:
                    return BuildSingleRowSubselect((AlgebraSingleRowSubselect)node);
                case AlgebraKind.ExistsSubselect:
                    return BuildExistsSubselect((AlgebraExistsSubselect)node);
                case AlgebraKind.AllAnySubselect:
                    return BuildAllAnySubselect((AlgebraAllAnySubselect)node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ShowPlanNode BuildConstant(AlgebraConstantNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode("Constant", properties, children);
        }

        private static ShowPlanNode BuildTable(AlgebraTableNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode("Table (" + node.Symbol + ")", properties, children);
        }

        private static ShowPlanNode BuildFilter(AlgebraFilterNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input), Build(node.Condition) };
            return new ShowPlanNode("Filter", properties, children);
        }

        private static ShowPlanNode BuildCompute(AlgebraComputeNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var input = new[] {Build(node.Input)};
            var aggregates = from d in node.DefinedValues
                             let dName = d.Value.Name
                             let dProperties = Enumerable.Empty<KeyValuePair<string, string>>()
                             let dChildren = new[] { Build(d.Expression) }
                             select new ShowPlanNode(dName, dProperties, dChildren);
            var children = input.Concat(aggregates);
            return new ShowPlanNode("Compute", properties, children);
        }

        private static ShowPlanNode BuildJoin(AlgebraJoinNode node)
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

            return new ShowPlanNode(node.JoinKind + "Join", properties, children);
        }

        private static ShowPlanNode BuildTop(AlgebraTopNode node)
        {
            var operatorName = string.Format("Top {0}{1}", node.Limit, node.WithTies ? " With Ties" : string.Empty);
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode(operatorName, properties, children);
        }

        private static ShowPlanNode BuildSort(AlgebraSortNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            var slots = string.Join(", ", node.ValueSlots.Select(v => v.Name));
            return new ShowPlanNode("Sort " + slots, properties, children);
        }

        private static ShowPlanNode BuildCombinedQuery(AlgebraCombinedQuery node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Right) };
            var slots = string.Join(", ", node.OutputValueSlots.Select(v => v.Name));
            var name = string.Format("{0} {1}", node.Combinator, slots);
            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildGroupByAndAggregation(AlgebraGroupByAndAggregation node)
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

        private static ShowPlanNode BuildProject(AlgebraProjectNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            var slots = string.Join(", ", node.Output.Select(v => v.Name));
            return new ShowPlanNode("Project " + slots, properties, children);
        }

        private static ShowPlanNode BuildUnaryExpression(AlgebraUnaryExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] {Build(node.Expression)};
            return new ShowPlanNode(node.Signature.Kind.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildBinaryExpression(AlgebraBinaryExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Right) };
            return new ShowPlanNode(node.Signature.Kind.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildLiteralExpression(AlgebraLiteralExpression node)
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

        private static ShowPlanNode BuildValueSlotExpression(AlgebraValueSlotExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode(node.ValueSlot.Name, properties, children, true);
        }

        private static ShowPlanNode BuildVariableExpression(AlgebraVariableExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildFunctionInvocationExpression(AlgebraFunctionInvocationExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = node.Arguments.Select(Build);
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildPropertyAccessExpression(AlgebraPropertyAccessExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Target) };
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildMethodInvocationExpression(AlgebraMethodInvocationExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] {Build(node.Target)}.Concat(node.Arguments.Select(Build));
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildConversionExpression(AlgebraConversionExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Expression) };
            return new ShowPlanNode("Conversion (" + node.Type.ToDisplayName() + ")", properties, children, true);
        }

        private static ShowPlanNode BuildIsNullExpression(AlgebraIsNullExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Expression) };
            return new ShowPlanNode("IsNull", properties, children, true);
        }

        private static ShowPlanNode BuildCaseExpression(AlgebraCaseExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var caseLabels = node.CaseLabels.Select(BuildCaseLabel);
            var elseLabel = new[] { Build(node.ElseExpression) };
            var children = caseLabels.Concat(elseLabel);
            return new ShowPlanNode("Case", properties, children, true);
        }

        private static ShowPlanNode BuildCaseLabel(AlgebraCaseLabel node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[]
                               {
                                   Build(node.Condition),
                                   Build(node.Result)
                               };
            return new ShowPlanNode("When", properties, children, true);
        }

        private static ShowPlanNode BuildSingleRowSubselect(AlgebraSingleRowSubselect node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Query)};
            return new ShowPlanNode("Subselect", properties, children, true);
        }

        private static ShowPlanNode BuildExistsSubselect(AlgebraExistsSubselect node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Query) };
            return new ShowPlanNode("Exists", properties, children, true);
        }

        private static ShowPlanNode BuildAllAnySubselect(AlgebraAllAnySubselect node)
        {
            // TODO: Extract actual kind (ALL or ANY)
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Expression), Build(node.Query) };
            return new ShowPlanNode("AllAny (" + node.Signature.Kind + ")", properties, children, true);
        }
    }
}