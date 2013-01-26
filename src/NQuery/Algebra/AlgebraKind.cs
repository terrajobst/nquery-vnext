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
        Project,

        // Expressions

        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        ValueSlotExpression,
        VariableExpression,
        FunctionInvocationExpression,
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