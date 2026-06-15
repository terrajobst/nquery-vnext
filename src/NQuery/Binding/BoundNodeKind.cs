namespace NQuery.Binding;

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
    ValueExpression,

    // Table references
    NamedTableReference,
    DerivedTableReference,
    JoinTableReference,
    ApplyTableReference,
    ErrorTable,

    // Queries
    SelectQuery,
    UnionQuery,
    IntersectOrExceptQuery,
    OrderedQuery,
    EmptyQuery,

    // Other
    Query,
    SelectColumn,
    WildcardSelectColumn,
    OrderByColumn,
    CommonTableExpression,
}
