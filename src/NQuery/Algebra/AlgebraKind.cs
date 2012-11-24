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
        Concat,
        BinaryQuery,

        // Expressions

        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        ColumnExpression,
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