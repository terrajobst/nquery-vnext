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
        ValueSlotExpression,

        // Relations
        TableRelation,
        DerivedTableRelation,
        JoinRelation,
        HashMatchRelation,
        ComputeRelation,
        FilterRelation,
        GroupByAndAggregationRelation,
        StreamAggregatesRelation,
        ConstantRelation,
        UnionRelation,
        ConcatenationRelation,
        IntersectOrExceptRelation,
        ProjectRelation,
        SortRelation,
        TopRelation,
        AssertRelation,
        TableSpoolPusher,
        TableSpoolPopper,

        // Other
        Query,
        SelectColumn,
        WildcardSelectColumn,
        OrderByColumn,
        CommonTableExpression,
    }
}