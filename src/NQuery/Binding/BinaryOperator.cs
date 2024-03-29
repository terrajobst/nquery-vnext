using System.Collections.Immutable;
using System.Reflection;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal static class BinaryOperator
    {
        private static readonly BinaryOperatorSignature[] BuiltInMultiplySignatures =
        {
            new(BinaryOperatorKind.Multiply, typeof(int)),
            new(BinaryOperatorKind.Multiply, typeof(uint)),
            new(BinaryOperatorKind.Multiply, typeof(long)),
            new(BinaryOperatorKind.Multiply, typeof(ulong)),
            new(BinaryOperatorKind.Multiply, typeof(float)),
            new(BinaryOperatorKind.Multiply, typeof(double)),
            new(BinaryOperatorKind.Multiply, BuiltInOperators.DecimalMultiplyMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInDivideSignatures =
        {
            new(BinaryOperatorKind.Divide, typeof(int)),
            new(BinaryOperatorKind.Divide, typeof(uint)),
            new(BinaryOperatorKind.Divide, typeof(long)),
            new(BinaryOperatorKind.Divide, typeof(ulong)),
            new(BinaryOperatorKind.Divide, typeof(float)),
            new(BinaryOperatorKind.Divide, typeof(double)),
            new(BinaryOperatorKind.Divide, BuiltInOperators.DecimalDivideMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInModulusSignatures =
        {
            new(BinaryOperatorKind.Modulus, typeof(int)),
            new(BinaryOperatorKind.Modulus, typeof(uint)),
            new(BinaryOperatorKind.Modulus, typeof(long)),
            new(BinaryOperatorKind.Modulus, typeof(ulong)),
            new(BinaryOperatorKind.Modulus, typeof(float)),
            new(BinaryOperatorKind.Modulus, typeof(double)),
            new(BinaryOperatorKind.Modulus, BuiltInOperators.DecimalModulusMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInAddSignatures =
        {
            new(BinaryOperatorKind.Add, typeof(int)),
            new(BinaryOperatorKind.Add, typeof(uint)),
            new(BinaryOperatorKind.Add, typeof(long)),
            new(BinaryOperatorKind.Add, typeof(ulong)),
            new(BinaryOperatorKind.Add, typeof(float)),
            new(BinaryOperatorKind.Add, typeof(double)),
            new(BinaryOperatorKind.Add, BuiltInOperators.StringConcatStringStringMethod),
            new(BinaryOperatorKind.Add, typeof(string), typeof(string), typeof(object), BuiltInOperators.StringConcatObjectObjectMethod),
            new(BinaryOperatorKind.Add, typeof(string), typeof(object), typeof(string), BuiltInOperators.StringConcatObjectObjectMethod),
            new(BinaryOperatorKind.Add, BuiltInOperators.DecimalAddMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInSubSignatures =
        {
            new(BinaryOperatorKind.Sub, typeof(int)),
            new(BinaryOperatorKind.Sub, typeof(uint)),
            new(BinaryOperatorKind.Sub, typeof(long)),
            new(BinaryOperatorKind.Sub, typeof(ulong)),
            new(BinaryOperatorKind.Sub, typeof(float)),
            new(BinaryOperatorKind.Sub, typeof(double)),
            new(BinaryOperatorKind.Sub, BuiltInOperators.DecimalSubtractMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInLeftShiftSignatures =
        {
            new(BinaryOperatorKind.LeftShift, typeof(int), typeof(int), typeof(int)),
            new(BinaryOperatorKind.LeftShift, typeof(uint), typeof(uint), typeof(int)),
            new(BinaryOperatorKind.LeftShift, typeof(long), typeof(long), typeof(int)),
            new(BinaryOperatorKind.LeftShift, typeof(ulong), typeof(ulong), typeof(int))
        };

        private static readonly BinaryOperatorSignature[] BuiltInRightShiftSignatures =
        {
            new(BinaryOperatorKind.RightShift, typeof(int)),
            new(BinaryOperatorKind.RightShift, typeof(uint), typeof(uint), typeof(int)),
            new(BinaryOperatorKind.RightShift, typeof(long), typeof(long), typeof(int)),
            new(BinaryOperatorKind.RightShift, typeof(ulong), typeof(ulong), typeof(int))
        };

        private static readonly BinaryOperatorSignature[] BuiltInEqualSignatures =
        {
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(int)),
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(uint)),
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(long)),
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(ulong)),
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(float)),
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(double)),
            new(BinaryOperatorKind.Equal, typeof(bool)),
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(object)),
            new(BinaryOperatorKind.Equal, typeof(bool), typeof(string)),
            new(BinaryOperatorKind.Equal, BuiltInOperators.DecimalEqualsMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInNotEqualSignatures =
        {
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(int)),
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(uint)),
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(long)),
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(ulong)),
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(float)),
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(double)),
            new(BinaryOperatorKind.NotEqual, typeof(bool)),
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(object)),
            new(BinaryOperatorKind.NotEqual, typeof(bool), typeof(string)),
            new(BinaryOperatorKind.NotEqual, BuiltInOperators.DecimalNotEqualsMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInLessSignatures =
        {
            new(BinaryOperatorKind.Less, typeof(bool), typeof(int)),
            new(BinaryOperatorKind.Less, typeof(bool), typeof(uint)),
            new(BinaryOperatorKind.Less, typeof(bool), typeof(long)),
            new(BinaryOperatorKind.Less, typeof(bool), typeof(ulong)),
            new(BinaryOperatorKind.Less, typeof(bool), typeof(float)),
            new(BinaryOperatorKind.Less, typeof(bool), typeof(double)),
            new(BinaryOperatorKind.Less, BuiltInOperators.DecimalLessMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInGreaterSignatures =
        {
            new(BinaryOperatorKind.Greater, typeof(bool), typeof(int)),
            new(BinaryOperatorKind.Greater, typeof(bool), typeof(uint)),
            new(BinaryOperatorKind.Greater, typeof(bool), typeof(long)),
            new(BinaryOperatorKind.Greater, typeof(bool), typeof(ulong)),
            new(BinaryOperatorKind.Greater, typeof(bool), typeof(float)),
            new(BinaryOperatorKind.Greater, typeof(bool), typeof(double)),
            new(BinaryOperatorKind.Greater, BuiltInOperators.DecimalGreaterMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInLessOrEqualSignatures =
        {
            new(BinaryOperatorKind.LessOrEqual, typeof(bool), typeof(int)),
            new(BinaryOperatorKind.LessOrEqual, typeof(bool), typeof(uint)),
            new(BinaryOperatorKind.LessOrEqual, typeof(bool), typeof(long)),
            new(BinaryOperatorKind.LessOrEqual, typeof(bool), typeof(ulong)),
            new(BinaryOperatorKind.LessOrEqual, typeof(bool), typeof(float)),
            new(BinaryOperatorKind.LessOrEqual, typeof(bool), typeof(double)),
            new(BinaryOperatorKind.LessOrEqual, BuiltInOperators.DecimalLessOrEqualMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInGreaterOrEqualSignatures =
        {
            new(BinaryOperatorKind.GreaterOrEqual, typeof(bool), typeof(int)),
            new(BinaryOperatorKind.GreaterOrEqual, typeof(bool), typeof(uint)),
            new(BinaryOperatorKind.GreaterOrEqual, typeof(bool), typeof(long)),
            new(BinaryOperatorKind.GreaterOrEqual, typeof(bool), typeof(ulong)),
            new(BinaryOperatorKind.GreaterOrEqual, typeof(bool), typeof(float)),
            new(BinaryOperatorKind.GreaterOrEqual, typeof(bool), typeof(double)),
            new(BinaryOperatorKind.GreaterOrEqual, BuiltInOperators.DecimalGreaterOrEqualMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInBitAndSignatures =
        {
            new(BinaryOperatorKind.BitAnd, typeof(int)),
            new(BinaryOperatorKind.BitAnd, typeof(uint)),
            new(BinaryOperatorKind.BitAnd, typeof(long)),
            new(BinaryOperatorKind.BitAnd, typeof(ulong)),
            new(BinaryOperatorKind.BitAnd, typeof(bool))
        };

        private static readonly BinaryOperatorSignature[] BuiltInBitOrSignatures =
        {
            new(BinaryOperatorKind.BitOr, typeof(int)),
            new(BinaryOperatorKind.BitOr, typeof(uint)),
            new(BinaryOperatorKind.BitOr, typeof(long)),
            new(BinaryOperatorKind.BitOr, typeof(ulong)),
            new(BinaryOperatorKind.BitOr, typeof(bool))
        };

        private static readonly BinaryOperatorSignature[] BuiltInBitXorSignatures =
        {
            new(BinaryOperatorKind.BitXor, typeof(int)),
            new(BinaryOperatorKind.BitXor, typeof(uint)),
            new(BinaryOperatorKind.BitXor, typeof(long)),
            new(BinaryOperatorKind.BitXor, typeof(ulong)),
            new(BinaryOperatorKind.BitXor, typeof(bool))
        };

        private static readonly BinaryOperatorSignature[] BuiltInLogicalAndSignatures =
        {
            new(BinaryOperatorKind.LogicalAnd, typeof(bool))
        };

        private static readonly BinaryOperatorSignature[] BuiltInLogicalOrSignatures =
        {
            new(BinaryOperatorKind.LogicalOr, typeof(bool))
        };

        private static readonly BinaryOperatorSignature[] BuiltInLikeSignatures =
        {
            new(BinaryOperatorKind.Like, BuiltInOperators.LikeMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInPowerSignatures =
        {
            new(BinaryOperatorKind.Power, BuiltInOperators.PowerMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInSimilarToSignatures =
        {
            new(BinaryOperatorKind.SimilarTo, BuiltInOperators.SimilarToMethod)
        };

        private static readonly BinaryOperatorSignature[] BuiltInSoundsLikeSignatures =
        {
            new(BinaryOperatorKind.SoundsLike, BuiltInOperators.SoundsLikeMethod)
        };

        internal static OverloadResolutionResult<BinaryOperatorSignature> Resolve(BinaryOperatorKind kind, Type leftOperandType, Type rightOperandType)
        {
            var result = ResolveOverloads(kind, leftOperandType, rightOperandType);

            var signature = result.Best;
            if (result.Best is null || !IsObjectComparison(signature.Signature))
                return result;

            // C# doesn't bind to reference equality unless both operands are considered reference types.

            if (leftOperandType.IsValueType || rightOperandType.IsValueType)
            {
                var newCandidates = result.Candidates.ToList();
                var i = newCandidates.IndexOf(result.Best);
                newCandidates[i] = newCandidates[i].MarkNotApplicable();
                return new OverloadResolutionResult<BinaryOperatorSignature>(null, null, newCandidates);
            }

            return result;
        }

        private static bool IsObjectComparison(BinaryOperatorSignature signature)
        {
            return (signature.Kind == BinaryOperatorKind.Equal || signature.Kind == BinaryOperatorKind.NotEqual) &&
                   signature.MethodInfo is null &&
                   signature.ReturnType == typeof(bool) &&
                   signature.GetParameterType(0) == typeof(object) &&
                   signature.GetParameterType(1) == typeof(object);
        }

        private static OverloadResolutionResult<BinaryOperatorSignature> ResolveOverloads(BinaryOperatorKind kind, Type leftOperandType, Type rightOperandType)
        {
            var builtInSignatures = GetBuiltInSignatures(kind);

            // If both types are built-in, we can simply perform the overload resolution
            // against the built-in signatures only.

            if (BothTypesBuiltIn(leftOperandType, rightOperandType))
                return OverloadResolution.Perform(builtInSignatures, leftOperandType, rightOperandType);

            // Otherwise, we need to consider user defined signatures.
            //
            // NOTE: We generally want to perform an overload resolution against the unified
            //       set of both, built-in signatures as well as user defined signatures.
            //       However, if the type provides an operator that is applicable, we want to
            //       to hide the built-in operators. In other words, in those cases the user
            //       defined operators shadows the built-in operators.
            //       Please note that we don't ask whether the overload resolution found a
            //       best match -- we just check if it has an applicable operator. This makes
            //       sure that any ambiguity errors will not include built-in operators
            //       in the output.

            var userDefinedSignatures = GetUserDefinedSignatures(kind, leftOperandType, rightOperandType);
            if (userDefinedSignatures.Any())
            {
                var userDefinedResult = OverloadResolution.Perform(userDefinedSignatures, leftOperandType, rightOperandType);
                if (userDefinedResult.Candidates.Any(c => c.IsApplicable))
                    return userDefinedResult;
            }

            var signatures = builtInSignatures.Concat(userDefinedSignatures);
            return OverloadResolution.Perform(signatures, leftOperandType, rightOperandType);
        }

        private static bool BothTypesBuiltIn(Type leftOperandType, Type rightOperandType)
        {
            return leftOperandType.GetKnownType() is not null &&
                   rightOperandType.GetKnownType() is not null;
        }

        private static IEnumerable<BinaryOperatorSignature> GetBuiltInSignatures(BinaryOperatorKind kind)
        {
            switch (kind)
            {
                case BinaryOperatorKind.Multiply:
                    return BuiltInMultiplySignatures;
                case BinaryOperatorKind.Divide:
                    return BuiltInDivideSignatures;
                case BinaryOperatorKind.Modulus:
                    return BuiltInModulusSignatures;
                case BinaryOperatorKind.Add:
                    return BuiltInAddSignatures;
                case BinaryOperatorKind.Sub:
                    return BuiltInSubSignatures;
                case BinaryOperatorKind.Equal:
                    return BuiltInEqualSignatures;
                case BinaryOperatorKind.NotEqual:
                    return BuiltInNotEqualSignatures;
                case BinaryOperatorKind.Less:
                    return BuiltInLessSignatures;
                case BinaryOperatorKind.LessOrEqual:
                    return BuiltInLessOrEqualSignatures;
                case BinaryOperatorKind.Greater:
                    return BuiltInGreaterSignatures;
                case BinaryOperatorKind.GreaterOrEqual:
                    return BuiltInGreaterOrEqualSignatures;
                case BinaryOperatorKind.BitXor:
                    return BuiltInBitXorSignatures;
                case BinaryOperatorKind.BitAnd:
                    return BuiltInBitAndSignatures;
                case BinaryOperatorKind.BitOr:
                    return BuiltInBitOrSignatures;
                case BinaryOperatorKind.LeftShift:
                    return BuiltInLeftShiftSignatures;
                case BinaryOperatorKind.RightShift:
                    return BuiltInRightShiftSignatures;
                case BinaryOperatorKind.LogicalAnd:
                    return BuiltInLogicalAndSignatures;
                case BinaryOperatorKind.LogicalOr:
                    return BuiltInLogicalOrSignatures;
                case BinaryOperatorKind.Power:
                    return BuiltInPowerSignatures;
                case BinaryOperatorKind.Like:
                    return BuiltInLikeSignatures;
                case BinaryOperatorKind.SimilarTo:
                    return BuiltInSimilarToSignatures;
                case BinaryOperatorKind.SoundsLike:
                    return BuiltInSoundsLikeSignatures;
                default:
                    throw ExceptionBuilder.UnexpectedValue(kind);
            }
        }

        private static ImmutableArray<BinaryOperatorSignature> GetUserDefinedSignatures(BinaryOperatorKind kind, Type leftOperandType, Type rightOperandType)
        {
            var methodName = GetOperatorMethodName(kind);
            return (from m in GetOperatorMethods(methodName, leftOperandType, rightOperandType)
                    where HasOperatorSignature(m)
                    select new BinaryOperatorSignature(kind, m)).ToImmutableArray();
        }

        private static string GetOperatorMethodName(BinaryOperatorKind kind)
        {
            switch (kind)
            {
                case BinaryOperatorKind.Power:
                    return @"op_Power";
                case BinaryOperatorKind.Multiply:
                    return @"op_Multiply";
                case BinaryOperatorKind.Divide:
                    return @"op_Division";
                case BinaryOperatorKind.Modulus:
                    return @"op_Modulus";
                case BinaryOperatorKind.Add:
                    return @"op_Addition";
                case BinaryOperatorKind.Sub:
                    return @"op_Subtraction";
                case BinaryOperatorKind.Equal:
                    return @"op_Equality";
                case BinaryOperatorKind.NotEqual:
                    return @"op_Inequality";
                case BinaryOperatorKind.Less:
                    return @"op_LessThan";
                case BinaryOperatorKind.LessOrEqual:
                    return @"op_LessThanOrEqual";
                case BinaryOperatorKind.Greater:
                    return @"op_GreaterThan";
                case BinaryOperatorKind.GreaterOrEqual:
                    return @"op_GreaterThanOrEqual";
                case BinaryOperatorKind.BitXor:
                    return @"op_ExclusiveOr";
                case BinaryOperatorKind.BitAnd:
                    return @"op_BitwiseAnd";
                case BinaryOperatorKind.BitOr:
                    return @"op_BitwiseOr";
                case BinaryOperatorKind.LeftShift:
                    return @"op_LeftShift";
                case BinaryOperatorKind.RightShift:
                    return @"op_RightShift";
                case BinaryOperatorKind.Like:
                    return @"op_Like";
                case BinaryOperatorKind.SimilarTo:
                    return @"op_SimilarTo";
                case BinaryOperatorKind.SoundsLike:
                    return @"op_SoundsLike";
                case BinaryOperatorKind.LogicalAnd:
                    return @"op_LogicalAnd";
                case BinaryOperatorKind.LogicalOr:
                    return @"op_LogicalOr";
                default:
                    throw ExceptionBuilder.UnexpectedValue(kind);
            }
        }

        private static IEnumerable<MethodInfo> GetOperatorMethods(string methodName, Type type, Type rightType)
        {
            return type == rightType
                       ? GetOperatorMethods(methodName, type)
                       : GetOperatorMethods(methodName, type).Concat(GetOperatorMethods(methodName, rightType));
        }

        private static IEnumerable<MethodInfo> GetOperatorMethods(string methodName, Type type)
        {
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                       .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal));
        }

        private static bool HasOperatorSignature(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType != typeof(void) &&
                   methodInfo.GetParameters().Length == 2;
        }
    }
}