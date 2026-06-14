#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Algebra
{
    // Builds a ShowPlanNode tree for the new pipeline's logical algebra, mirroring the
    // legacy ShowPlanBuilder (which renders the legacy BoundRelation tree). The physical
    // side has its own builder; both share BuildExpression because the row-expression
    // language (LogicalExpression) is the same in both layers.
    internal static class LogicalShowPlanBuilder
    {
        public static ShowPlan Build(string name, LogicalQuery query)
        {
            return new ShowPlan(name, BuildQuery(query));
        }

        private static ShowPlanNode BuildQuery(LogicalQuery query)
        {
            var slots = string.Join(@", ", query.OutputColumns.Select(c => c.Name));
            var children = new[] { Build(query.Root) };
            return new ShowPlanNode(@"Query " + slots, NoProperties, children);
        }

        private static ShowPlanNode Build(LogicalOperator node)
        {
            switch (node.Kind)
            {
                case LogicalOperatorKind.Empty:
                    return new ShowPlanNode(@"Empty", NoProperties, NoChildren);
                case LogicalOperatorKind.Constant:
                    return new ShowPlanNode(@"Constant", NoProperties, NoChildren);
                case LogicalOperatorKind.TableScan:
                    return BuildTableScan((LogicalTableScan)node);
                case LogicalOperatorKind.Filter:
                    return BuildFilter((LogicalFilter)node);
                case LogicalOperatorKind.Compute:
                    return BuildCompute((LogicalCompute)node);
                case LogicalOperatorKind.Project:
                    return BuildProject((LogicalProject)node);
                case LogicalOperatorKind.Join:
                    return BuildJoin((LogicalJoin)node);
                case LogicalOperatorKind.Apply:
                    return BuildApply((LogicalApply)node);
                case LogicalOperatorKind.Aggregate:
                    return BuildAggregate((LogicalAggregate)node);
                case LogicalOperatorKind.Union:
                    return BuildUnion((LogicalUnion)node);
                case LogicalOperatorKind.IntersectOrExcept:
                    return BuildIntersectOrExcept((LogicalIntersectOrExcept)node);
                case LogicalOperatorKind.Sort:
                    return BuildSort((LogicalSort)node);
                case LogicalOperatorKind.Top:
                    return BuildTop((LogicalTop)node);
                case LogicalOperatorKind.Assert:
                    return BuildAssert((LogicalAssert)node);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private static ShowPlanNode BuildTableScan(LogicalTableScan node)
        {
            var name = node.TableInstance.Table.Name;
            var columns = string.Join(@", ", node.OutputValueSlots.Select(s => s.Name));
            return new ShowPlanNode($"Table ({name}), DefinedValues := {columns}", NoProperties, NoChildren);
        }

        private static ShowPlanNode BuildFilter(LogicalFilter node)
        {
            var children = new[] { Build(node.Input) }.Concat(node.Conditions.Select(BuildExpression));
            return new ShowPlanNode(@"Filter", NoProperties, children);
        }

        private static ShowPlanNode BuildCompute(LogicalCompute node)
        {
            var children = new[] { Build(node.Input) }.Concat(node.DefinedValues.Select(BuildComputedValue));
            return new ShowPlanNode(@"Compute", NoProperties, children);
        }

        private static ShowPlanNode BuildProject(LogicalProject node)
        {
            var slots = string.Join(@", ", node.Outputs.Select(s => s.Name));
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode($"Project {slots}", NoProperties, children);
        }

        private static ShowPlanNode BuildJoin(LogicalJoin node)
        {
            var probe = node.Probe is null ? string.Empty : $", Probe := {node.Probe.Name}";
            var name = $"{node.JoinKind}Join{probe}";
            var children = new[] { Build(node.Left), Build(node.Right) }
                .Concat(node.Conditions.Select(BuildExpression));
            if (node.PassthruPredicate is not null)
                children = children.Append(BuildLabeled(@"Passthru", node.PassthruPredicate));
            return new ShowPlanNode(name, NoProperties, children);
        }

        private static ShowPlanNode BuildApply(LogicalApply node)
        {
            var outerReferences = node.OuterReferences.IsEmpty
                                      ? string.Empty
                                      : $" Outer References := [{string.Join(@", ", node.OuterReferences.Select(s => s.Name))}]";
            var probe = node.Probe is null ? string.Empty : $", Probe := {node.Probe.Name}";
            var name = $"{node.ApplyKind}Apply{outerReferences}{probe}";
            var children = new[] { Build(node.Left), Build(node.Right) };
            if (node.Passthru is not null)
                return new ShowPlanNode(name, NoProperties, children.Append(BuildLabeled(@"Passthru", node.Passthru)));
            return new ShowPlanNode(name, NoProperties, children);
        }

        private static ShowPlanNode BuildAggregate(LogicalAggregate node)
        {
            var slots = string.Join(@", ", node.Groups.Select(g => g.ValueSlot.Name));
            var children = new[] { Build(node.Input) }.Concat(node.Aggregates.Select(BuildAggregatedValue));
            return new ShowPlanNode($"Aggregate {slots}", NoProperties, children);
        }

        private static ShowPlanNode BuildUnion(LogicalUnion node)
        {
            var kind = node.IsUnionAll ? @"UnionAll" : @"Union";
            var outputs = BuildUnifiedValues(node.DefinedValues);
            var children = node.Inputs.Select(Build);
            return new ShowPlanNode($"{kind} {outputs}", NoProperties, children);
        }

        private static ShowPlanNode BuildIntersectOrExcept(LogicalIntersectOrExcept node)
        {
            var name = node.IsIntersect ? @"Intersect" : @"Except";
            var children = new[] { Build(node.Left), Build(node.Right) };
            return new ShowPlanNode(name, NoProperties, children);
        }

        private static ShowPlanNode BuildSort(LogicalSort node)
        {
            var op = node.IsDistinct ? @"DistinctSort" : @"Sort";
            var slots = string.Join(@", ", node.SortedValues.Select(v => v.ValueSlot.Name));
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode($"{op} {slots}", NoProperties, children);
        }

        private static ShowPlanNode BuildTop(LogicalTop node)
        {
            var ties = string.Join(@", ", node.TieEntries.Select(v => v.ValueSlot.Name));
            var name = $"Top {node.Limit}{(node.TieEntries.IsEmpty ? string.Empty : $" With Ties ({ties})")}";
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode(name, NoProperties, children);
        }

        private static ShowPlanNode BuildAssert(LogicalAssert node)
        {
            var children = new[] { Build(node.Input), BuildExpression(node.Condition) };
            return new ShowPlanNode(@"Assert", NoProperties, children);
        }

        private static ShowPlanNode BuildComputedValue(LogicalComputedValue value)
        {
            var children = new[] { BuildExpression(value.Expression) };
            return new ShowPlanNode(value.ValueSlot.Name, NoProperties, children);
        }

        private static ShowPlanNode BuildAggregatedValue(LogicalAggregatedValue value)
        {
            var children = new[] { BuildExpression(value.Argument) };
            return new ShowPlanNode($"{value.Output.Name} = {value.Aggregate.Name}", NoProperties, children);
        }

        private static string BuildUnifiedValues(ImmutableArray<LogicalUnifiedValue> definedValues)
        {
            return string.Join(@", ", definedValues.Select(d => $"{d.ValueSlot.Name} := [{string.Join(@", ", d.InputValueSlots.Select(i => i.Name))}]"));
        }

        // A relational-shaped node whose single child is an expression, used to attach a
        // labeled predicate (e.g. a join's passthru) to an operator.
        private static ShowPlanNode BuildLabeled(string label, LogicalExpression expression)
        {
            return new ShowPlanNode(label, NoProperties, new[] { BuildExpression(expression) });
        }

        public static ShowPlanNode BuildExpression(LogicalExpression node)
        {
            switch (node.Kind)
            {
                case LogicalExpressionKind.Literal:
                    return BuildLiteral((LogicalLiteralExpression)node);
                case LogicalExpressionKind.ValueSlot:
                    return new ShowPlanNode(((LogicalValueSlotExpression)node).ValueSlot.Name, NoProperties, NoChildren, true);
                case LogicalExpressionKind.Variable:
                    return new ShowPlanNode(((LogicalVariableExpression)node).Symbol.ToString(), NoProperties, NoChildren, true);
                case LogicalExpressionKind.Unary:
                    return BuildUnary((LogicalUnaryExpression)node);
                case LogicalExpressionKind.Binary:
                    return BuildBinary((LogicalBinaryExpression)node);
                case LogicalExpressionKind.Conversion:
                    return BuildConversion((LogicalConversionExpression)node);
                case LogicalExpressionKind.IsNull:
                    return new ShowPlanNode(@"IsNull", NoProperties, new[] { BuildExpression(((LogicalIsNullExpression)node).Expression) }, true);
                case LogicalExpressionKind.Case:
                    return BuildCase((LogicalCaseExpression)node);
                case LogicalExpressionKind.FunctionInvocation:
                    return BuildFunctionInvocation((LogicalFunctionInvocationExpression)node);
                case LogicalExpressionKind.PropertyAccess:
                    return BuildPropertyAccess((LogicalPropertyAccessExpression)node);
                case LogicalExpressionKind.MethodInvocation:
                    return BuildMethodInvocation((LogicalMethodInvocationExpression)node);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private static ShowPlanNode BuildLiteral(LogicalLiteralExpression node)
        {
            var value = node.Value is null
                            ? @"NULL"
                            : node.Value is string
                                  ? @"'" + node.Value.ToString()!.Replace(@"'", @"''") + @"'"
                                  : node.Value.ToString();
            return new ShowPlanNode($"Literal ({value})", NoProperties, NoChildren, true);
        }

        private static ShowPlanNode BuildUnary(LogicalUnaryExpression node)
        {
            var children = new[] { BuildExpression(node.Expression) };
            return new ShowPlanNode(node.OperatorKind.ToString(), NoProperties, children, true);
        }

        private static ShowPlanNode BuildBinary(LogicalBinaryExpression node)
        {
            var children = new[] { BuildExpression(node.Left), BuildExpression(node.Right) };
            return new ShowPlanNode(node.OperatorKind.ToString(), NoProperties, children, true);
        }

        private static ShowPlanNode BuildConversion(LogicalConversionExpression node)
        {
            var children = new[] { BuildExpression(node.Expression) };
            return new ShowPlanNode($"Conversion ({node.Type.ToDisplayName()})", NoProperties, children, true);
        }

        private static ShowPlanNode BuildCase(LogicalCaseExpression node)
        {
            var caseLabels = node.CaseLabels.Select(BuildCaseLabel);
            var elseLabel = node.ElseExpression is null
                                ? Enumerable.Empty<ShowPlanNode>()
                                : new[] { BuildExpression(node.ElseExpression) };
            var children = caseLabels.Concat(elseLabel);
            return new ShowPlanNode(@"Case", NoProperties, children, true);
        }

        private static ShowPlanNode BuildCaseLabel(LogicalCaseLabel node)
        {
            var children = new[] { BuildExpression(node.Condition), BuildExpression(node.ThenExpression) };
            return new ShowPlanNode(@"When", NoProperties, children, true);
        }

        private static ShowPlanNode BuildFunctionInvocation(LogicalFunctionInvocationExpression node)
        {
            var name = node.Symbol is null ? @"?" : node.Symbol.ToString();
            var children = node.Arguments.Select(BuildExpression);
            return new ShowPlanNode(name, NoProperties, children, true);
        }

        private static ShowPlanNode BuildPropertyAccess(LogicalPropertyAccessExpression node)
        {
            var children = new[] { BuildExpression(node.Target) };
            return new ShowPlanNode(node.Symbol.ToString(), NoProperties, children, true);
        }

        private static ShowPlanNode BuildMethodInvocation(LogicalMethodInvocationExpression node)
        {
            var name = node.Symbol is null ? @"?" : node.Symbol.ToString();
            var children = new[] { BuildExpression(node.Target) }.Concat(node.Arguments.Select(BuildExpression));
            return new ShowPlanNode(name, NoProperties, children, true);
        }

        private static IEnumerable<KeyValuePair<string, string>> NoProperties => Enumerable.Empty<KeyValuePair<string, string>>();

        private static IEnumerable<ShowPlanNode> NoChildren => Enumerable.Empty<ShowPlanNode>();
    }
}
