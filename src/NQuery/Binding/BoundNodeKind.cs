using System;

namespace NQuery.Binding
{
    internal enum BoundNodeKind
    {
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        VariableExpression,
        FunctionInvocationExpression,
        AggregateExpression,
        PropertyAccessExpression,
        MethodInvocationExpression,
        ConversionExpression,
        IsNullExpression,
        CaseLabel,
        CaseExpression,
        SingleRowSubselect,
        ExistsSubselect,
        AllAnySubselect,
        ValueSlotExpression,
        NamedTableReference,
        DerivedTableReference,
        JoinedTableReference,
        SelectColumn,
        WildcardSelectColumn,
        OrderByColumn,
        SelectQuery,
        CombinedQuery,
        CommonTableExpression,
        CommonTableExpressionQuery,
        OrderedQuery,
        TopQuery,
    }
}