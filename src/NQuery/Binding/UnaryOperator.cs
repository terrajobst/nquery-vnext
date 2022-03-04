using System.Collections.Immutable;
using System.Reflection;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal static class UnaryOperator
    {
        private static readonly UnaryOperatorSignature[] BuiltInIdentitySignatures =
        {
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(int), typeof(int)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(uint), typeof(uint)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(long), typeof(long)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(ulong), typeof(ulong)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(float), typeof(float)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, typeof(double), typeof(double)),
            new UnaryOperatorSignature(UnaryOperatorKind.Identity, BuiltInOperators.DecimalUnaryIdentityMethod)
        };

        private static readonly UnaryOperatorSignature[] BuiltInNegationSignatures =
        {
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(int), typeof(int)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(long), typeof(long)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(float), typeof(float)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, typeof(double), typeof(double)),
            new UnaryOperatorSignature(UnaryOperatorKind.Negation, BuiltInOperators.DecimalUnaryNegationMethod)
        };

        private static readonly UnaryOperatorSignature[] BuiltInComplementSignatures =
        {
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(int), typeof(int)),
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(uint), typeof(uint)),
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(long), typeof(long)),
            new UnaryOperatorSignature(UnaryOperatorKind.Complement, typeof(ulong), typeof(ulong))
        };

        private static readonly UnaryOperatorSignature[] BuiltInLogicalNotSignatures =
        {
            new UnaryOperatorSignature(UnaryOperatorKind.LogicalNot, typeof(bool), typeof(bool))
        };

        internal static OverloadResolutionResult<UnaryOperatorSignature> Resolve(UnaryOperatorKind kind, Type type)
        {
            var builtInSignatures = GetBuiltInSignatures(kind);

            // If the type is built-in, we can simply perform the overload resolution
            // against the built-in signatures only.

            if (TypeBuiltIn(type))
                return OverloadResolution.Perform(builtInSignatures, type);

            // Otherwise, we need to consider user defined signatures.
            //
            // NOTE: We generally want to perform an overload resolution against the unified
            //       set of both, built-in signatures as well as user defined signatures.
            //       However, if the type provides an operator that is applicable, we want
            //       to hide the built-in operators. In other words, in those cases the user
            //       defined operators shadows the built-in operators.
            //       Please note that we don't ask whether the overload resolution found a
            //       best match -- we just check if it has an applicable operator. This makes
            //       sure that any ambiguity errors will not include built-in operators
            //       in the output.

            var userDefinedSignatures = GetUserDefinedSignatures(kind, type);
            if (userDefinedSignatures.Any())
            {
                var userDefinedResult = OverloadResolution.Perform(userDefinedSignatures, type);
                if (userDefinedResult.Candidates.Any(c => c.IsApplicable))
                    return userDefinedResult;
            }

            var signatures = builtInSignatures.Concat(userDefinedSignatures);
            return OverloadResolution.Perform(signatures, type);
        }

        private static bool TypeBuiltIn(Type type)
        {
            return type.GetKnownType() is not null;
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
                    throw ExceptionBuilder.UnexpectedValue(kind);
            }
        }

        private static ImmutableArray<UnaryOperatorSignature> GetUserDefinedSignatures(UnaryOperatorKind kind, Type type)
        {
            var methodName = GetOperatorMethodName(kind);
            return (from m in GetOperatorMethods(methodName, type)
                    where HasOperatorSignature(m)
                    select new UnaryOperatorSignature(kind, m)).ToImmutableArray();
        }

        private static string GetOperatorMethodName(UnaryOperatorKind kind)
        {
            switch (kind)
            {
                case UnaryOperatorKind.Identity:
                    return @"op_UnaryPlus";
                case UnaryOperatorKind.Negation:
                    return @"op_UnaryNegation";
                case UnaryOperatorKind.Complement:
                    return @"op_OnesComplement";
                case UnaryOperatorKind.LogicalNot:
                    return @"op_LogicalNot";
                default:
                    throw ExceptionBuilder.UnexpectedValue(kind);
            }
        }

        private static IEnumerable<MethodInfo> GetOperatorMethods(string methodName, Type type)
        {
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                       .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal));
        }

        private static bool HasOperatorSignature(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType != typeof(void) &&
                   methodInfo.GetParameters().Length == 1;
        }
    }
}