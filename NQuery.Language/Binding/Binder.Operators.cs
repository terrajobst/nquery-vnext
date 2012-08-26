using System;
using System.Reflection;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
        private static BoundUnaryOperatorKind GetBoundUnaryOperator(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.ComplementExpression:
                    return BoundUnaryOperatorKind.Complement;
                case SyntaxKind.IdentityExpression:
                    return BoundUnaryOperatorKind.Identity;
                case SyntaxKind.NegationExpression:
                    return BoundUnaryOperatorKind.Negation;
                case SyntaxKind.LogicalNotExpression:
                    return BoundUnaryOperatorKind.LogicalNot;
                default:
                    throw new ArgumentOutOfRangeException("kind");
            }
        }

        private static BoundBinaryOperatorKind GetBoundBinaryOperator(SyntaxKind expressionKind)
        {
            switch (expressionKind)
            {
                case SyntaxKind.BitAndExpression:
                    return BoundBinaryOperatorKind.BitAnd;
                case SyntaxKind.BitOrExpression:
                    return BoundBinaryOperatorKind.BitOr;
                case SyntaxKind.BitXorExpression:
                    return BoundBinaryOperatorKind.BitXor;
                case SyntaxKind.AddExpression:
                    return BoundBinaryOperatorKind.Add;
                case SyntaxKind.SubExpression:
                    return BoundBinaryOperatorKind.Sub;
                case SyntaxKind.MultiplyExpression:
                    return BoundBinaryOperatorKind.Multiply;
                case SyntaxKind.DivideExpression:
                    return BoundBinaryOperatorKind.Divide;
                case SyntaxKind.ModulusExpression:
                    return BoundBinaryOperatorKind.Modulus;
                case SyntaxKind.PowerExpression:
                    return BoundBinaryOperatorKind.Power;
                case SyntaxKind.EqualExpression:
                    return BoundBinaryOperatorKind.Equal;
                case SyntaxKind.NotEqualExpression:
                    return BoundBinaryOperatorKind.NotEqual;
                case SyntaxKind.LessExpression:
                    return BoundBinaryOperatorKind.Less;
                case SyntaxKind.NotGreaterExpression:
                case SyntaxKind.LessOrEqualExpression:
                    return BoundBinaryOperatorKind.LessOrEqual;
                case SyntaxKind.GreaterExpression:
                    return BoundBinaryOperatorKind.Greater;
                case SyntaxKind.NotLessExpression:
                case SyntaxKind.GreaterOrEqualExpression:
                    return BoundBinaryOperatorKind.GreaterOrEqual;
                case SyntaxKind.LeftShiftExpression:
                    return BoundBinaryOperatorKind.LeftShift;
                case SyntaxKind.RightShiftExpression:
                    return BoundBinaryOperatorKind.RightShift;
                case SyntaxKind.LogicalAndExpression:
                    return BoundBinaryOperatorKind.LogicalAnd;
                case SyntaxKind.LogicalOrExpression:
                    return BoundBinaryOperatorKind.LogicalOr;
                case SyntaxKind.LikeExpression:
                    return BoundBinaryOperatorKind.Like;
                case SyntaxKind.SoundslikeExpression:
                    return BoundBinaryOperatorKind.Soundex;
                case SyntaxKind.SimilarToExpression:
                    return BoundBinaryOperatorKind.SimilarTo;
                default:
                    throw new ArgumentOutOfRangeException("expressionKind");
            }
        }

        private MethodInfo BindUnaryOperator(Type type, object operatorKind)
        {
            // TODO: Bind unary operator
            return null;
        }

        private MethodInfo BindBinaryOperator(Type leftType, Type rightType, BoundBinaryOperatorKind operatorKind)
        {
            // TODO: Bind binary operator
            return null;
        }
    }
}