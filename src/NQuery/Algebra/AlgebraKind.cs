using System;

namespace NQuery.Algebra
{
    internal enum AlgebraKind
    {
        // Nodes

        Constant,
        Table,
        Join,
        Filter,
        Compute,
        Top,
        Sort,
        BinaryQuery,
        GroupByAndAggregation,

        // Expressions

        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        ValueSlotExpression,
        VariableExpression,
        FunctionInvocationExpression,
        AggregateExpression,
        PropertyAccessExpression,
        MethodInvocationExpression,
        ConversionExpression,
        IsNullExpression,
        CaseExpression,
        SingleRowSubselect,
        ExistsSubselect,
        AllAnySubselect,
    }
}