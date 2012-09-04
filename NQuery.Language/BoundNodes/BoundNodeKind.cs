namespace NQuery.Language.BoundNodes
{
    internal enum BoundNodeKind
    {
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        VariableExpression,
        FunctionInvocationExpression,
        PropertyAccessExpression,
        MethodInvocationExpression,
        CaseLabel,
        CaseExpression,
        SingleRowSubselect,
        ExistsSubselect,
        AllAnySubselect,
        NamedTableReference,
        DerivedTableReference,
        JoinedTableReference,
        SelectColumn,
        SelectQuery,
        CombinedQuery,
    }
}