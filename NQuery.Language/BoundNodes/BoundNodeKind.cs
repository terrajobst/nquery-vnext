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
        NamedTableReference,
        DerivedTableReference,
        JoinedTableReference,
        SelectColumn,
        SelectQuery,
        CombinedQuery,
    }
}