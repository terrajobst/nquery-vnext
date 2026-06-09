#nullable enable

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.LogicalOptimization
{
    // Collects the value slots *referenced* by a logical expression or operator
    // subtree (as opposed to the slots they define). Used to decide whether a
    // subtree depends on another's output -- e.g. whether an Apply's right side
    // still references the left (correlation), or which join side a selection
    // belongs to.
    internal static class LogicalSlotReferenceFinder
    {
        public static ISet<ValueSlot> FindReferencedSlots(LogicalExpression expression)
        {
            var slots = new HashSet<ValueSlot>();
            AddExpression(expression, slots);
            return slots;
        }

        public static ISet<ValueSlot> FindReferencedSlots(LogicalOperator node)
        {
            var slots = new HashSet<ValueSlot>();
            AddOperator(node, slots);
            return slots;
        }

        private static void AddOperator(LogicalOperator node, HashSet<ValueSlot> slots)
        {
            switch (node.Kind)
            {
                case LogicalOperatorKind.Empty:
                    AddEmpty((LogicalEmpty)node, slots);
                    break;
                case LogicalOperatorKind.Constant:
                    AddConstant((LogicalConstant)node, slots);
                    break;
                case LogicalOperatorKind.TableScan:
                    AddTableScan((LogicalTableScan)node, slots);
                    break;
                case LogicalOperatorKind.Filter:
                    AddFilter((LogicalFilter)node, slots);
                    break;
                case LogicalOperatorKind.Compute:
                    AddCompute((LogicalCompute)node, slots);
                    break;
                case LogicalOperatorKind.Project:
                    AddProject((LogicalProject)node, slots);
                    break;
                case LogicalOperatorKind.Join:
                    AddJoin((LogicalJoin)node, slots);
                    break;
                case LogicalOperatorKind.Apply:
                    AddApply((LogicalApply)node, slots);
                    break;
                case LogicalOperatorKind.Aggregate:
                    AddAggregate((LogicalAggregate)node, slots);
                    break;
                case LogicalOperatorKind.Union:
                    AddUnion((LogicalUnion)node, slots);
                    break;
                case LogicalOperatorKind.IntersectOrExcept:
                    AddIntersectOrExcept((LogicalIntersectOrExcept)node, slots);
                    break;
                case LogicalOperatorKind.Sort:
                    AddSort((LogicalSort)node, slots);
                    break;
                case LogicalOperatorKind.Top:
                    AddTop((LogicalTop)node, slots);
                    break;
                case LogicalOperatorKind.Assert:
                    AddAssert((LogicalAssert)node, slots);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private static void AddEmpty(LogicalEmpty node, HashSet<ValueSlot> slots)
        {
        }

        private static void AddConstant(LogicalConstant node, HashSet<ValueSlot> slots)
        {
        }

        private static void AddTableScan(LogicalTableScan node, HashSet<ValueSlot> slots)
        {
        }

        private static void AddFilter(LogicalFilter node, HashSet<ValueSlot> slots)
        {
            foreach (var condition in node.Conditions)
                AddExpression(condition, slots);
            AddOperator(node.Input, slots);
        }

        private static void AddCompute(LogicalCompute node, HashSet<ValueSlot> slots)
        {
            foreach (var value in node.DefinedValues)
                AddExpression(value.Expression, slots);
            AddOperator(node.Input, slots);
        }

        private static void AddProject(LogicalProject node, HashSet<ValueSlot> slots)
        {
            foreach (var output in node.Outputs)
                slots.Add(output);
            AddOperator(node.Input, slots);
        }

        private static void AddJoin(LogicalJoin node, HashSet<ValueSlot> slots)
        {
            foreach (var condition in node.Conditions)
                AddExpression(condition, slots);
            AddExpression(node.PassthruPredicate, slots);
            AddOperator(node.Left, slots);
            AddOperator(node.Right, slots);
        }

        private static void AddApply(LogicalApply node, HashSet<ValueSlot> slots)
        {
            AddOperator(node.Left, slots);
            AddOperator(node.Right, slots);
        }

        private static void AddAggregate(LogicalAggregate node, HashSet<ValueSlot> slots)
        {
            foreach (var group in node.Groups)
                slots.Add(group.ValueSlot);
            foreach (var aggregate in node.Aggregates)
                AddExpression(aggregate.Argument, slots);
            AddOperator(node.Input, slots);
        }

        private static void AddUnion(LogicalUnion node, HashSet<ValueSlot> slots)
        {
            foreach (var value in node.DefinedValues)
                foreach (var input in value.InputValueSlots)
                    slots.Add(input);
            foreach (var input in node.Inputs)
                AddOperator(input, slots);
        }

        private static void AddIntersectOrExcept(LogicalIntersectOrExcept node, HashSet<ValueSlot> slots)
        {
            AddOperator(node.Left, slots);
            AddOperator(node.Right, slots);
        }

        private static void AddSort(LogicalSort node, HashSet<ValueSlot> slots)
        {
            foreach (var sorted in node.SortedValues)
                slots.Add(sorted.ValueSlot);
            AddOperator(node.Input, slots);
        }

        private static void AddTop(LogicalTop node, HashSet<ValueSlot> slots)
        {
            foreach (var tie in node.TieEntries)
                slots.Add(tie.ValueSlot);
            AddOperator(node.Input, slots);
        }

        private static void AddAssert(LogicalAssert node, HashSet<ValueSlot> slots)
        {
            AddExpression(node.Condition, slots);
            AddOperator(node.Input, slots);
        }

        private static void AddExpression(LogicalExpression? node, HashSet<ValueSlot> slots)
        {
            if (node is null)
                return;

            switch (node.Kind)
            {
                case LogicalExpressionKind.Literal:
                    AddLiteral((LogicalLiteralExpression)node, slots);
                    break;
                case LogicalExpressionKind.ValueSlot:
                    AddValueSlot((LogicalValueSlotExpression)node, slots);
                    break;
                case LogicalExpressionKind.Variable:
                    AddVariable((LogicalVariableExpression)node, slots);
                    break;
                case LogicalExpressionKind.Unary:
                    AddUnary((LogicalUnaryExpression)node, slots);
                    break;
                case LogicalExpressionKind.Binary:
                    AddBinary((LogicalBinaryExpression)node, slots);
                    break;
                case LogicalExpressionKind.Conversion:
                    AddConversion((LogicalConversionExpression)node, slots);
                    break;
                case LogicalExpressionKind.IsNull:
                    AddIsNull((LogicalIsNullExpression)node, slots);
                    break;
                case LogicalExpressionKind.Case:
                    AddCase((LogicalCaseExpression)node, slots);
                    break;
                case LogicalExpressionKind.FunctionInvocation:
                    AddFunctionInvocation((LogicalFunctionInvocationExpression)node, slots);
                    break;
                case LogicalExpressionKind.PropertyAccess:
                    AddPropertyAccess((LogicalPropertyAccessExpression)node, slots);
                    break;
                case LogicalExpressionKind.MethodInvocation:
                    AddMethodInvocation((LogicalMethodInvocationExpression)node, slots);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private static void AddLiteral(LogicalLiteralExpression node, HashSet<ValueSlot> slots)
        {
        }

        private static void AddValueSlot(LogicalValueSlotExpression node, HashSet<ValueSlot> slots)
        {
            slots.Add(node.ValueSlot);
        }

        private static void AddVariable(LogicalVariableExpression node, HashSet<ValueSlot> slots)
        {
        }

        private static void AddUnary(LogicalUnaryExpression node, HashSet<ValueSlot> slots)
        {
            AddExpression(node.Expression, slots);
        }

        private static void AddBinary(LogicalBinaryExpression node, HashSet<ValueSlot> slots)
        {
            AddExpression(node.Left, slots);
            AddExpression(node.Right, slots);
        }

        private static void AddConversion(LogicalConversionExpression node, HashSet<ValueSlot> slots)
        {
            AddExpression(node.Expression, slots);
        }

        private static void AddIsNull(LogicalIsNullExpression node, HashSet<ValueSlot> slots)
        {
            AddExpression(node.Expression, slots);
        }

        private static void AddCase(LogicalCaseExpression node, HashSet<ValueSlot> slots)
        {
            foreach (var label in node.CaseLabels)
            {
                AddExpression(label.Condition, slots);
                AddExpression(label.ThenExpression, slots);
            }
            AddExpression(node.ElseExpression, slots);
        }

        private static void AddFunctionInvocation(LogicalFunctionInvocationExpression node, HashSet<ValueSlot> slots)
        {
            foreach (var argument in node.Arguments)
                AddExpression(argument, slots);
        }

        private static void AddPropertyAccess(LogicalPropertyAccessExpression node, HashSet<ValueSlot> slots)
        {
            AddExpression(node.Target, slots);
        }

        private static void AddMethodInvocation(LogicalMethodInvocationExpression node, HashSet<ValueSlot> slots)
        {
            AddExpression(node.Target, slots);
            foreach (var argument in node.Arguments)
                AddExpression(argument, slots);
        }
    }
}
