using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Algebra
{
    internal static class ShowPlanBuilder
    {
        private static string GetOperatorName(AlgebraJoinKind algebraJoinKind)
        {
            switch (algebraJoinKind)
            {
                case AlgebraJoinKind.Inner:
                    return "INNER JOIN";
                case AlgebraJoinKind.LeftOuter:
                    return "LEFT OUTER JOIN";
                case AlgebraJoinKind.LeftSemiJoin:
                    return "LEFT SEMI JOIN";
                case AlgebraJoinKind.LeftAntiSemiJoin:
                    return "LEFT ANTI SEMI JOIN";
                default:
                    throw new ArgumentOutOfRangeException("algebraJoinKind");
            }
        }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ShowPlanNode BuildConstant(AlgebraConstantNode node)
        {
            return new ShowPlanNode("CONSTANT", Enumerable.Empty<KeyValuePair<string, string>>(), Enumerable.Empty<ShowPlanNode>());
        }

        private static ShowPlanNode BuildTable(AlgebraTableNode node)
        {
            var tableName = string.Format("{0} AS {1}", node.Symbol.Table.Name, node.Symbol.Name);
            var properties = new[]
                                 {
                                     new KeyValuePair<string, string>("Table", tableName)
                                 };
            return new ShowPlanNode("TABLE", properties, Enumerable.Empty<ShowPlanNode>());
        }

        private static ShowPlanNode BuildFilter(AlgebraFilterNode node)
        {
            var properties = node.Condition == null
                                 ? Enumerable.Empty<KeyValuePair<string, string>>()
                                 : new[] {new KeyValuePair<string, string>("Condition", node.Condition.ToString())};
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode("FILTER", properties, children);
        }

        private static ShowPlanNode BuildCompute(AlgebraComputeNode node)
        {
            var properties = node.ResultExpressions.Select((e, i) => new KeyValuePair<string, string>(i.ToString(), e.ToString()));
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode("COMPUTE", properties, children);
        }

        private static ShowPlanNode BuildJoin(AlgebraJoinNode node)
        {
            var operatorName = GetOperatorName(node.JoinKind);

            var probeProperty = node.ProbeValue == null
                                    ? Enumerable.Empty<KeyValuePair<string, string>>()
                                    : new[] {new KeyValuePair<string, string>("Probe", node.ProbeValue.DisplayName)};

            var conditionProperty = node.Condition == null
                                        ? Enumerable.Empty<KeyValuePair<string, string>>()
                                        : new[] {new KeyValuePair<string, string>("Condition", node.Condition.ToString())};

            var properties = probeProperty.Concat(conditionProperty);

            var children = new[]
                               {
                                   Build(node.Left),
                                   Build(node.Right)
                               };

            return new ShowPlanNode(operatorName, properties, children);
        }

        private static ShowPlanNode BuildTop(AlgebraTopNode node)
        {
            var operatorName = string.Format("TOP {0}{1}", node.Top, node.WithTies ? " WITH TIES" : string.Empty);
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode(operatorName, properties, children);
        }

        private static ShowPlanNode BuildConcat(AlgebraConcatNode node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = node.Inputs.Select(Build);
            return new ShowPlanNode("CONCAT", properties, children);
        }
    }
}