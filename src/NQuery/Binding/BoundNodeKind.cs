using System;

namespace NQuery.Binding
{
    internal enum BoundNodeKind
    {
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

        // Algebra
        ComputeRelation,
        FilterRelation,
        GroupByAndAggregationRelation,
        TableRelation,
        JoinRelation,
        ConstantRelation,
        CombinedRelation,
        ProjectRelation,
        SortRelation,
        TopRelation,
        DerivedTableRelation
    }
}