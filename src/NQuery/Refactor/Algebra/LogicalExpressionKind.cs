#nullable enable

namespace NQuery.Refactor.Algebra
{
    internal enum LogicalExpressionKind
    {
        Literal,
        ValueSlot,
        Variable,
        Unary,
        Binary,
        Conversion,
        IsNull,
        Case,
        FunctionInvocation,
        PropertyAccess,
        MethodInvocation
    }
}
