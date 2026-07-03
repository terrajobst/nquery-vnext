using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

internal sealed class ReflectionMethodDefinition : MethodDefinition
{
    public ReflectionMethodDefinition(MethodInfo methodInfo, string name)
        : base(name, methodInfo.ReturnType, ConvertParameters(methodInfo))
    {
        ThrowIfNull(methodInfo);
        ThrowIfNull(name);

        MethodInfo = methodInfo;
    }

    private static IEnumerable<ParameterDefinition> ConvertParameters(MethodInfo methodInfo)
    {
        return methodInfo.GetParameters()
                         .Select(p => ParameterDefinition.Create(p.Name!, p.ParameterType))
                         .ToImmutableArray();
    }

    internal override Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
    {
        // The declared parameter types are erased (Nullable<T> -> T), and the engine lowers
        // arguments to their non-nullable form before calling, so a lowered `int` may arrive for
        // a real `int?` parameter. Expression.Call requires exact assignability, so coerce each
        // argument back to the method's actual parameter type.
        return Expression.Call(instance, MethodInfo, CoerceArguments.ToParameters(MethodInfo, arguments));
    }

    public MethodInfo MethodInfo { get; }
}
