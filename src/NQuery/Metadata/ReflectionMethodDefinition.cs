using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

internal sealed class ReflectionMethodDefinition : MethodDefinition
{
    public ReflectionMethodDefinition(MethodInfo methodInfo, string name)
        : base(name, methodInfo.ReturnType, ConvertParameters(methodInfo))
    {
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
        return Expression.Call(instance, MethodInfo, arguments);
    }

    public MethodInfo MethodInfo { get; }
}
