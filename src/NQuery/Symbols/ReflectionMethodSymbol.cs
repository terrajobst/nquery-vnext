using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Symbols
{
    public class ReflectionMethodSymbol : MethodSymbol
    {
        public ReflectionMethodSymbol(MethodInfo methodInfo)
            : this(methodInfo, methodInfo?.Name)
        {
        }

        public ReflectionMethodSymbol(MethodInfo methodInfo, string name)
            : base(name, methodInfo?.ReturnType, ConvertParameters(methodInfo))
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            MethodInfo = methodInfo;
        }

        private static IEnumerable<ParameterSymbol> ConvertParameters(MethodInfo methodInfo)
        {
            return (methodInfo?.GetParameters().Select(p => new ReflectionParameterSymbol(p)) ?? Enumerable.Empty<ParameterSymbol>()).ToImmutableArray();
        }

        public override Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
        {
            return Expression.Call(instance, MethodInfo, arguments);
        }

        public MethodInfo MethodInfo { get; }
    }
}