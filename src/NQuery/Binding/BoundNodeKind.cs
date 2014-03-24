using System;

namespace NQuery.Binding
{
    internal enum BoundNodeKind
    {
        // Expressions
        ErrorExpression,
        TableExpression,
        ColumnExpression,
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
        CaseExpression,
        SingleRowSubselect,
        ExistsSubselect,
        AllAnySubselect,
        ValueSlotExpression,

        // Relations
        TableRelation,
        DerivedTableRelation,
        JoinRelation,
        ComputeRelation,
        FilterRelation,
        GroupByAndAggregationRelation,
        StreamAggregatesRelation,
        ConstantRelation,
        CombinedRelation,
        ProjectRelation,
        SortRelation,
        TopRelation,

        // Other
        Query,
        SelectColumn,
        WildcardSelectColumn,
        OrderByColumn,
        CommonTableExpression,
    }
}