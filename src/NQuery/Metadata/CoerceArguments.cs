using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

// Bridges the erased metadata view of a member back to its real CLR signature. The type system
// erases Nullable<T> to T, and the engine lowers arguments to their non-nullable form before an
// invocation, so a lowered `int` can arrive for a real `int?` parameter. Expression.Call requires
// each argument to be exactly assignable to the parameter type, so each argument is converted to
// the member's actual parameter type (a no-op when they already match).
internal static class CoerceArguments
{
    public static IEnumerable<Expression> ToParameters(MethodBase method, IEnumerable<Expression> arguments)
    {
        var parameters = method.GetParameters();
        var args = arguments.ToImmutableArray();

        var result = new Expression[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            var argument = args[i];
            var parameterType = parameters[i].ParameterType;
            result[i] = argument.Type == parameterType
                ? argument
                : Expression.Convert(argument, parameterType);
        }

        return result;
    }
}
