using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal static class UnaryOperator
    {
        private static readonly UnaryOperatorSignature[] BuiltInIdentitySignatures = new[]
        {
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(int), typeof(int)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(uint), typeof(uint)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(long), typeof(long)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(ulong), typeof(ulong)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(float), typeof(float)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(double), typeof(double)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, BuiltInOperators.DecimalUnaryIdentityMethod)
        };

        private static readonly UnaryOperatorSignature[] BuiltInNegationSignatures = new[]
        {
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(int), typeof(int)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(long), typeof(long)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(float), typeof(float)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(double), typeof(double)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, BuiltInOperators.DecimalUnaryNegationMethod)
        };

        private static readonly UnaryOperatorSignature[] BuiltInComplementSignatures = new[]
        {
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(int), typeof(int)),
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(uint), typeof(uint)),
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(long), typeof(long)),
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(ulong), typeof(ulong))
        };

        private static readonly UnaryOperatorSignature[] BuiltInLogicalNotSignatures = new[]
        {
            new UnaryOperatorSignature(UnaryOperatorKind.LogicalNot, typeof(bool), typeof(bool))
        };

        internal static OverloadResolutionResult<UnaryOperatorSignature> Resolve(UnaryOperatorKind kind, Type type)
        {
            var methodName = GetMethodName(kind);
            var signatures = (from m in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                              where IsValidOperator(m, methodName)
                              select new UnaryOperatorSignature(kind, m)).ToImmutableArray();

            if (signatures.Length > 0)
                return GetOperator(signatures, type);

            var builtInSignatures = GetBuiltInSignatures(kind);
            return GetOperator(builtInSignatures, type);
        }

        private static IEnumerable<UnaryOperatorSignature> GetBuiltInSignatures(UnaryOperatorKind kind)
        {
            switch (kind)
            {
                case UnaryOperatorKind.Identity:
                    return BuiltInIdentitySignatures;
                case UnaryOperatorKind.Negation:
                    return BuiltInNegationSignatures;
                case UnaryOperatorKind.Complement:
                    return BuiltInComplementSignatures;
                case UnaryOperatorKind.LogicalNot:
                    return BuiltInLogicalNotSignatures;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static OverloadResolutionResult<UnaryOperatorSignature> GetOperator(IEnumerable<UnaryOperatorSignature> signatures, Type operandType)
        {
            return OverloadResolution.Perform(signatures, operandType);
        }

        private static bool IsValidOperator(MethodInfo methodInfo, string methodName)
        {
            if (methodInfo.ReturnType == typeof(void))
                return false;

            if (methodInfo.GetParameters().Length != 1)
                return false;

            return methodInfo.Name.Equals(methodName, StringComparison.Ordinal);
        }

        private static string GetMethodName(UnaryOperatorKind kind)
        {
            switch (kind)
            {
                case UnaryOperatorKind.Identity:
                    return "op_UnaryPlus";
                case UnaryOperatorKind.Negation:
                    return "op_UnaryNegation";
                case UnaryOperatorKind.Complement:
                    return "op_OnesComplement";
                case UnaryOperatorKind.LogicalNot:
                    return "op_LogicalNot";
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}