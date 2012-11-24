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
                case AlgebraKind.Concat:
                    return BuildConcat((AlgebraConcatNode)node);
                case AlgebraKind.BinaryQuery:
                    return BuildBinaryQuery((AlgebraBinaryQueryNode)node);
                case AlgebraKind.UnaryExpression:
                    return BuildUnaryExpression((AlgebraUnaryExpression)node);
                case AlgebraKind.BinaryExpression:
                    return BuildBinaryExpression((AlgebraBinaryExpression)node);
                case AlgebraKind.LiteralExpression:
                    return BuildLiteralExpression((AlgebraLiteralExpression)node);
                case AlgebraKind.ColumnExpression:
                    return BuildColumnExpression((AlgebraColumnExpression)node);
                case AlgebraKind.VariableExpression:
                    return BuildVariableExpression((AlgebraVariableExpression)node);
                case AlgebraKind.FunctionInvocationExpression:
                    return BuildFunctionInvocationExpression((AlgebraFunctionInvocationExpression)node);
                case AlgebraKind.AggregateExpression:
                    return BuildAggregateExpression((AlgebraAggregateExpression)node);
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
            var children = new[] {Build(node.Input)}.Concat(node.Expressions.Select(Build));
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
            var operatorName = string.Format("Top {0}{1}", node.Top, node.WithTies ? " With Ties" : string.Empty);
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode(operatorName, properties, children);
        }

        private static ShowPlanNode BuildConcat(AlgebraConcatNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = node.Inputs.Select(Build);
            return new ShowPlanNode("Concat", properties, children);
        }

        private static ShowPlanNode BuildBinaryQuery(AlgebraBinaryQueryNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Right) };
            return new ShowPlanNode(node.Combinator.ToString(), properties, children);
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
            return new ShowPlanNode("Literal (" + node.Value + ")", properties, children, true);
        }

        private static ShowPlanNode BuildColumnExpression(AlgebraColumnExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode(node.Symbol.ToString(), properties, children, true);
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

        private static ShowPlanNode BuildAggregateExpression(AlgebraAggregateExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] {Build(node.Argument)};
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
            return new ShowPlanNode("Conversion", properties, children, true);
        }

        private static ShowPlanNode BuildIsNullExpression(AlgebraIsNullExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Expression) };
            return new ShowPlanNode("IsNull", properties, children, true);
        }

        private static ShowPlanNode BuildCaseExpression(AlgebraCaseExpression node)
        {
            // TODO: Handle CASE
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode("Case", properties, children, true);
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