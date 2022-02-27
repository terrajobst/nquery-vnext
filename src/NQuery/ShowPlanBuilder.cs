using System.Text;

using NQuery.Binding;
using NQuery.Optimization;

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
                case BoundNodeKind.UnionRelation:
                    return BuildUnionRelation((BoundUnionRelation)node);
                case BoundNodeKind.ConcatenationRelation:
                    return BuildConcatenationRelation((BoundConcatenationRelation)node);
                case BoundNodeKind.IntersectOrExceptRelation:
                    return BuildIntersectOrExceptRelation((BoundIntersectOrExceptRelation)node);
                case BoundNodeKind.GroupByAndAggregationRelation:
                    return BuildGroupByAndAggregation((BoundGroupByAndAggregationRelation)node);
                case BoundNodeKind.StreamAggregatesRelation:
                    return BuildStreamAggregatesRelation((BoundStreamAggregatesRelation)node);
                case BoundNodeKind.ProjectRelation:
                    return BuildProject((BoundProjectRelation)node);
                case BoundNodeKind.AssertRelation:
                    return BuildAssert((BoundAssertRelation)node);
                case BoundNodeKind.TableSpoolPusher:
                    return BuildTableSpoolPusher((BoundTableSpoolPusher)node);
                case BoundNodeKind.TableSpoolPopper:
                    return BuildTableSpoolPopper((BoundTableSpoolPopper)node);
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
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private static ShowPlanNode BuildQueryRelation(BoundQuery node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation) };
            var slots = string.Join(@", ", node.OutputColumns.Select(v => v.Name));
            return new ShowPlanNode(@"Query " + slots, properties, children);
        }

        private static ShowPlanNode BuildConstant(BoundConstantRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            return new ShowPlanNode(@"Constant", properties, children);
        }

        private static ShowPlanNode BuildTable(BoundTableRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            var name = node.TableInstance.Table.Name;
            var columns = string.Join(@", ", node.DefinedValues.Select(d => d.ValueSlot.Name));
            return new ShowPlanNode($"Table ({name}), DefinedValues := {columns}", properties, children);
        }

        private static ShowPlanNode BuildDerivedTable(BoundDerivedTableRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation) };
            return new ShowPlanNode($"Derived Table ({node.TableInstance})", properties, children);
        }

        private static ShowPlanNode BuildFilter(BoundFilterRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input), Build(node.Condition) };
            return new ShowPlanNode(@"Filter", properties, children);
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
            return new ShowPlanNode(@"Compute", properties, children);
        }

        private static ShowPlanNode BuildJoin(BoundJoinRelation node)
        {
            var outerReferences = GetOuterReferences(node);
            var probe = node.Probe is null ? string.Empty : $", ProbeColumn := {node.Probe}";
            var passthru = node.PassthruPredicate is null ? string.Empty : $", Passthru := {node.PassthruPredicate}";
            var name = $"{node.JoinType}Join{outerReferences}{probe}{passthru}";
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var leftAndRight = new[] { Build(node.Left), Build(node.Right) };
            var children = node.Condition is null
                               ? leftAndRight
                               : leftAndRight.Concat(new[] {Build(node.Condition)});

            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildHashMatch(BoundHashMatchRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var leftAndRight = new[]
                               {
                                   Build(node.Build),
                                   Build(node.Probe)
                               };

            var children = node.Remainder is null
                               ? leftAndRight
                               : leftAndRight.Concat(new[] { Build(node.Remainder) });

            return new ShowPlanNode($"Hash Match ({node.LogicalOperator}) [{node.BuildKey} = {node.ProbeKey}]", properties, children);
        }

        private static ShowPlanNode BuildTop(BoundTopRelation node)
        {
            var tieEntries = string.Join(@", ", node.TieEntries.Select(v => v.ValueSlot.Name));
            var operatorName = $"Top {node.Limit}{(!node.TieEntries.Any() ? string.Empty : $" With Ties ({tieEntries})")}";
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode(operatorName, properties, children);
        }

        private static ShowPlanNode BuildSort(BoundSortRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            var slots = string.Join(@", ", node.SortedValues.Select(v => v.ValueSlot.Name));
            var op = node.IsDistinct ? @"DistinctSort" : @"Sort";
            var name = $"{op} {slots}";
            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildUnionRelation(BoundUnionRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = node.Inputs.Select(Build);
            var kind = node.IsUnionAll ? @"UnionAll" : @"Union";
            var outputs = string.Join(@", ", node.DefinedValues.Select(d => $"{d.ValueSlot.Name} := [{string.Join(@", ", d.InputValueSlots.Select(i => i.Name))}]"));
            var name = $"{kind} {outputs}";
            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildConcatenationRelation(BoundConcatenationRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = node.Inputs.Select(Build);
            var outputs = string.Join(@", ", node.DefinedValues.Select(d => $"{d.ValueSlot.Name} := [{string.Join(@", ", d.InputValueSlots.Select(i => i.Name))}]"));
            var name = $"Concatenation {outputs}";
            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildIntersectOrExceptRelation(BoundIntersectOrExceptRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Right) };
            var name = node.IsIntersect ? @"Intersect" : @"Except";
            return new ShowPlanNode(name, properties, children);
        }

        private static ShowPlanNode BuildGroupByAndAggregation(BoundGroupByAndAggregationRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var input = new[] { Build(node.Input) };
            var aggregates = from a in node.Aggregates
                             let aName = $"{a.Output} = {a.Aggregate.Name}"
                             let aProperties = Enumerable.Empty<KeyValuePair<string, string>>()
                             let aChildren = new[] {Build(a.Argument)}
                             select new ShowPlanNode(aName, aProperties, aChildren);
            var children = input.Concat(aggregates);
            var slots = string.Join(@", ", node.Groups.Select(g => g.ValueSlot.Name));
            return new ShowPlanNode($"GroupByAndAggregation {slots}", properties, children);
        }

        private static ShowPlanNode BuildStreamAggregatesRelation(BoundStreamAggregatesRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var input = new[] { Build(node.Input) };
            var aggregates = from a in node.Aggregates
                             let aName = $"{a.Output} = {a.Aggregate.Name}"
                             let aProperties = Enumerable.Empty<KeyValuePair<string, string>>()
                             let aChildren = new[] { Build(a.Argument) }
                             select new ShowPlanNode(aName, aProperties, aChildren);
            var children = input.Concat(aggregates);
            var slots = string.Join(@", ", node.Groups.Select(g => g.ValueSlot.Name));
            return new ShowPlanNode($"StreamAggregates {slots}", properties, children);
        }

        private static ShowPlanNode BuildProject(BoundProjectRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            var slots = string.Join(@", ", node.Outputs.Select(v => v.Name));
            return new ShowPlanNode($"Project {slots}", properties, children);
        }

        private static ShowPlanNode BuildAssert(BoundAssertRelation node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input), Build(node.Condition) };
            return new ShowPlanNode(@"Assert", properties, children);
        }

        private static ShowPlanNode BuildTableSpoolPusher(BoundTableSpoolPusher node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Input) };
            return new ShowPlanNode(@"TableSpoolPusher", properties, children);
        }

        private static ShowPlanNode BuildTableSpoolPopper(BoundTableSpoolPopper node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            var slots = string.Join(@", ", node.Outputs.Select(v => v.Name));
            return new ShowPlanNode($"TableSpoolPopper {slots}", properties, children);
        }

        private static ShowPlanNode BuildUnaryExpression(BoundUnaryExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] {Build(node.Expression)};
            return new ShowPlanNode(node.OperatorKind.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildBinaryExpression(BoundBinaryExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Left), Build(node.Right) };
            return new ShowPlanNode(node.OperatorKind.ToString(), properties, children, true);
        }

        private static ShowPlanNode BuildLiteralExpression(BoundLiteralExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = Enumerable.Empty<ShowPlanNode>();
            var value = node.Value is null
                            ? @"NULL"
                            : node.Value is string
                                  ? @"'" + node.Value.ToString().Replace(@"'", @"''") + @"'"
                                  : node.Value.ToString();
            return new ShowPlanNode($"Literal ({value})", properties, children, true);
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
            return new ShowPlanNode($"Conversion ({node.Type.ToDisplayName()})", properties, children, true);
        }

        private static ShowPlanNode BuildIsNullExpression(BoundIsNullExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Expression) };
            return new ShowPlanNode(@"IsNull", properties, children, true);
        }

        private static ShowPlanNode BuildCaseExpression(BoundCaseExpression node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var caseLabels = node.CaseLabels.Select(BuildCaseLabel);
            var elseLabel = node.ElseExpression is null
                                ? Enumerable.Empty<ShowPlanNode>()
                                : new[] {Build(node.ElseExpression)};
            var children = caseLabels.Concat(elseLabel);
            return new ShowPlanNode(@"Case", properties, children, true);
        }

        private static ShowPlanNode BuildCaseLabel(BoundCaseLabel node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[]
                               {
                                   Build(node.Condition),
                                   Build(node.ThenExpression)
                               };
            return new ShowPlanNode(@"When", properties, children, true);
        }

        private static ShowPlanNode BuildSingleRowSubselect(BoundSingleRowSubselect node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation)};
            return new ShowPlanNode(@"Subselect", properties, children, true);
        }

        private static ShowPlanNode BuildExistsSubselect(BoundExistsSubselect node)
        {
            var properties = Enumerable.Empty<KeyValuePair<string, string>>();
            var children = new[] { Build(node.Relation) };
            return new ShowPlanNode(@"Exists", properties, children, true);
        }

        private static string GetOuterReferences(BoundJoinRelation node)
        {
            var outerReferences = OuterReferenceFinder.GetOuterReferences(node);

            var sb = new StringBuilder();

            foreach (var valueSlot in outerReferences)
            {
                sb.Append(sb.Length == 0 ? @" Outer References := [" : @", ");
                sb.Append(valueSlot.Name);
            }

            if (sb.Length == 0)
                return string.Empty;

            sb.Append(@"]");
            return sb.ToString();
        }
    }
}