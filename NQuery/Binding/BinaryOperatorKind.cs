using System;

namespace NQuery.Binding
{
    internal enum BinaryOperatorKind
    {
        Power,
        Multiply,
        Divide,
        Modulus,
        Add,
        Sub,
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
        BitXor,
        BitAnd,
        BitOr,
        LeftShift,
        RightShift,
        Like,
        SimilarTo,
        Soundslike,
        LogicalAnd,
        LogicalOr
    }
}