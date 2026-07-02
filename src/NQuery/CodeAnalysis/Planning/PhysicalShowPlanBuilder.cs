using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Planning;

// Builds a ShowPlanNode tree for the physical plan. The relational operators differ
// from the logical layer (algorithm choices: nested loops vs hash match, stream
// aggregate, concatenation), but the row-expression language is shared,
// so expression rendering is delegated to LogicalShowPlanBuilder.BuildExpression.
internal static class PhysicalShowPlanBuilder
{
    public static ShowPlan Build(string name, PhysicalQuery query)
    {
        var slots = string.Join(@", ", query.OutputColumns.Select(c => c.Name));
        var children = new[] { Build(query.Root) };
        return new ShowPlan(name, new ShowPlanNode(@"Query " + slots, NoProperties, children));
    }

    private static ShowPlanNode Build(PhysicalOperator node)
    {
        switch (node.Kind)
        {
            case PhysicalOperatorKind.Empty:
                return new ShowPlanNode(@"Empty", NoProperties, NoChildren);
            case PhysicalOperatorKind.Constant:
                return new ShowPlanNode(@"Constant", NoProperties, NoChildren);
            case PhysicalOperatorKind.TableScan:
                return BuildTableScan((PhysicalTableScan)node);
            case PhysicalOperatorKind.Filter:
                return BuildFilter((PhysicalFilter)node);
            case PhysicalOperatorKind.ComputeScalar:
                return BuildComputeScalar((PhysicalComputeScalar)node);
            case PhysicalOperatorKind.Project:
                return BuildProject((PhysicalProject)node);
            case PhysicalOperatorKind.NestedLoops:
                return BuildNestedLoops((PhysicalNestedLoops)node);
            case PhysicalOperatorKind.HashMatch:
                return BuildHashMatch((PhysicalHashMatch)node);
            case PhysicalOperatorKind.StreamAggregates:
                return BuildStreamAggregates((PhysicalStreamAggregates)node);
            case PhysicalOperatorKind.Sort:
                return BuildSort((PhysicalSort)node);
            case PhysicalOperatorKind.Top:
                return BuildTop((PhysicalTop)node);
            case PhysicalOperatorKind.Concatenation:
                return BuildConcatenation((PhysicalConcatenation)node);
            case PhysicalOperatorKind.RecursiveUnion:
                return BuildRecursiveUnion((PhysicalRecursiveUnion)node);
            case PhysicalOperatorKind.RecursiveReference:
                return BuildRecursiveReference((PhysicalRecursiveReference)node);
            case PhysicalOperatorKind.IndexSpool:
                return BuildIndexSpool((PhysicalIndexSpool)node);
            case PhysicalOperatorKind.Assert:
                return BuildAssert((PhysicalAssert)node);
            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }

    private static ShowPlanNode BuildTableScan(PhysicalTableScan node)
    {
        var name = node.TableInstance.Table.Name;
        var columns = string.Join(@", ", node.OutputValueSlots.Select(s => s.Name));
        return new ShowPlanNode($"Table ({name}), DefinedValues := {columns}", NoProperties, NoChildren);
    }

    private static ShowPlanNode BuildFilter(PhysicalFilter node)
    {
        var children = new[] { Build(node.Input) }.Concat(node.Conditions.Select(LogicalShowPlanBuilder.BuildExpression));
        return new ShowPlanNode(@"Filter", NoProperties, children);
    }

    private static ShowPlanNode BuildComputeScalar(PhysicalComputeScalar node)
    {
        var children = new[] { Build(node.Input) }.Concat(node.DefinedValues.Select(BuildComputedValue));
        return new ShowPlanNode(@"Compute Scalar", NoProperties, children);
    }

    private static ShowPlanNode BuildProject(PhysicalProject node)
    {
        var slots = string.Join(@", ", node.Outputs.Select(s => s.Name));
        var children = new[] { Build(node.Input) };
        return new ShowPlanNode($"Project {slots}", NoProperties, children);
    }

    private static ShowPlanNode BuildNestedLoops(PhysicalNestedLoops node)
    {
        var outerReferences = node.OuterReferences.IsEmpty
                                  ? string.Empty
                                  : $" Outer References := [{string.Join(@", ", node.OuterReferences.Select(s => s.Name))}]";
        var probe = node.Probe is null ? string.Empty : $", Probe := {node.Probe.Name}";
        var name = $"Nested Loops ({node.JoinKind}){outerReferences}{probe}";
        var children = new[] { Build(node.Left), Build(node.Right) }
            .Concat(node.Conditions.Select(LogicalShowPlanBuilder.BuildExpression));
        if (node.PassthruPredicate is not null)
            children = children.Append(BuildLabeled(@"Passthru", node.PassthruPredicate));
        return new ShowPlanNode(name, NoProperties, children);
    }

    private static ShowPlanNode BuildHashMatch(PhysicalHashMatch node)
    {
        var name = $"Hash Match ({node.HashMatchKind}) [{node.BuildKey.Name} = {node.ProbeKey.Name}]";
        var children = new[] { Build(node.Build), Build(node.Probe) }
            .Concat(node.Remainder.Select(LogicalShowPlanBuilder.BuildExpression));
        return new ShowPlanNode(name, NoProperties, children);
    }

    private static ShowPlanNode BuildStreamAggregates(PhysicalStreamAggregates node)
    {
        var slots = string.Join(@", ", node.Groups.Select(g => g.ValueSlot.Name));
        var children = new[] { Build(node.Input) }.Concat(node.Aggregates.Select(BuildAggregatedValue));
        return new ShowPlanNode($"Stream Aggregates {slots}", NoProperties, children);
    }

    private static ShowPlanNode BuildSort(PhysicalSort node)
    {
        var op = node.IsDistinct ? @"DistinctSort" : @"Sort";
        var slots = string.Join(@", ", node.SortedValues.Select(v => v.ValueSlot.Name));
        var children = new[] { Build(node.Input) };
        return new ShowPlanNode($"{op} {slots}", NoProperties, children);
    }

    private static ShowPlanNode BuildTop(PhysicalTop node)
    {
        var ties = string.Join(@", ", node.TieEntries.Select(v => v.ValueSlot.Name));
        var name = $"Top {node.Limit}{(node.TieEntries.IsEmpty ? string.Empty : $" With Ties ({ties})")}";
        var children = new[] { Build(node.Input) };
        return new ShowPlanNode(name, NoProperties, children);
    }

    private static ShowPlanNode BuildConcatenation(PhysicalConcatenation node)
    {
        var outputs = string.Join(@", ", node.DefinedValues.Select(d => $"{d.ValueSlot.Name} := [{string.Join(@", ", d.InputValueSlots.Select(i => i.Name))}]"));
        var children = node.Inputs.Select(Build);
        return new ShowPlanNode($"Concatenation {outputs}", NoProperties, children);
    }

    private static ShowPlanNode BuildRecursiveUnion(PhysicalRecursiveUnion node)
    {
        var outputs = string.Join(@", ", node.DefinedValues.Select(d => $"{d.ValueSlot.Name} := [{string.Join(@", ", d.InputValueSlots.Select(i => i.Name))}]"));
        var children = new[] { Build(node.Anchor) }.Concat(node.RecursiveMembers.Select(Build));
        return new ShowPlanNode($"Recursive Union ({node.Token.Name}) {outputs}", NoProperties, children);
    }

    private static ShowPlanNode BuildRecursiveReference(PhysicalRecursiveReference node)
    {
        var columns = string.Join(@", ", node.OutputValueSlots.Select(s => s.Name));
        return new ShowPlanNode($"Recursive Reference ({node.Token.Name}), DefinedValues := {columns}", NoProperties, NoChildren);
    }

    private static ShowPlanNode BuildIndexSpool(PhysicalIndexSpool node)
    {
        var name = $"Index Spool (Lazy) [{node.IndexKey.Name} = {node.ProbeKey.Name}]";
        var children = new[] { Build(node.Input) };
        return new ShowPlanNode(name, NoProperties, children);
    }

    private static ShowPlanNode BuildAssert(PhysicalAssert node)
    {
        var children = new[] { Build(node.Input), LogicalShowPlanBuilder.BuildExpression(node.Condition) };
        return new ShowPlanNode(@"Assert", NoProperties, children);
    }

    private static ShowPlanNode BuildComputedValue(LogicalComputedValue value)
    {
        var children = new[] { LogicalShowPlanBuilder.BuildExpression(value.Expression) };
        return new ShowPlanNode(value.ValueSlot.Name, NoProperties, children);
    }

    private static ShowPlanNode BuildAggregatedValue(LogicalAggregatedValue value)
    {
        var children = new[] { LogicalShowPlanBuilder.BuildExpression(value.Argument) };
        return new ShowPlanNode($"{value.Output.Name} = {value.Aggregate.Name}", NoProperties, children);
    }

    private static ShowPlanNode BuildLabeled(string label, LogicalExpression expression)
    {
        return new ShowPlanNode(label, NoProperties, new[] { LogicalShowPlanBuilder.BuildExpression(expression) });
    }

    private static IEnumerable<KeyValuePair<string, string>> NoProperties => [];

    private static IEnumerable<ShowPlanNode> NoChildren => [];
}
