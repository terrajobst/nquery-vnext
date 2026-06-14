using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

public abstract class MethodDefinition
{
    private protected MethodDefinition(string name, Type returnType, IEnumerable<ParameterDefinition> parameters)
    {
        ThrowIfNull(name);
        ThrowIfNull(returnType);
        ThrowIfNull(parameters);

        Name = name;
        ReturnType = returnType;
        Parameters = parameters.ToImmutableArray();
    }

    public string Name { get; }

    public Type ReturnType { get; }

    public ImmutableArray<ParameterDefinition> Parameters { get; }

    public IEnumerable<Type> GetParameterTypes()
    {
        return from p in Parameters
               select p.Type;
    }

    internal abstract Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments);

    public static MethodDefinition Create(MethodInfo methodInfo)
    {
        ThrowIfNull(methodInfo);

        return Create(methodInfo, methodInfo.Name);
    }

    public static MethodDefinition Create(MethodInfo methodInfo, string name)
    {
        ThrowIfNull(methodInfo);
        ThrowIfNull(name);

        return new ReflectionMethodDefinition(methodInfo, name);
    }
}
