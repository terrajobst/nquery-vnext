using System.Linq.Expressions;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public abstract class AggregateDefinition
{
    private protected AggregateDefinition()
    {
    }

    public abstract string Name { get; }

    // Resolves the fold this aggregate uses for a concrete argument type, or null if the
    // argument type isn't supported. The binder uses this to bind aggregate invocations.
    internal abstract AggregateFold? CreateFold(Type argumentType);

    // Defines a custom aggregate. The binder is called once per concrete argument type and
    // returns the fold to use for that type, or null if the aggregate doesn't support it.
    public static AggregateDefinition Create(string name, Func<Type, AggregateFold?> binder)
    {
        ThrowIfNull(name);
        ThrowIfNull(binder);

        return new FoldAggregateDefinition(name, binder);
    }

    // Defines a custom aggregate from a strongly-typed fold: a seed producing the initial
    // accumulator, an accumulate step folding one TValue into it, and a result selector. The
    // aggregate accepts any argument type NQuery can convert to TValue (and rejects the rest).
    public static AggregateDefinition Create<TValue, TState, TResult>(
        string name,
        System.Linq.Expressions.Expression<Func<TState>> seed,
        System.Linq.Expressions.Expression<Func<TState, TValue, TState>> accumulate,
        System.Linq.Expressions.Expression<Func<TState, TResult>> result)
    {
        ThrowIfNull(name);
        ThrowIfNull(seed);
        ThrowIfNull(accumulate);
        ThrowIfNull(result);

        return Create(name, argumentType =>
        {
            var input = BindInput(accumulate, argumentType);
            return input is null ? null : new AggregateFold(seed, input, result);
        });
    }

    // Adapts the TValue-typed accumulate step to the actual argument type: identity when they
    // match, otherwise the argument is converted to TValue (null if no conversion exists).
    private static LambdaExpression? BindInput<TState, TValue>(System.Linq.Expressions.Expression<Func<TState, TValue, TState>> accumulate, Type argumentType)
    {
        if (argumentType == typeof(TValue))
            return accumulate;

        var conversion = Conversion.Classify(argumentType, typeof(TValue));
        if (!conversion.Exists)
            return null;

        var method = conversion.ConversionMethods is [var conversionMethod] ? conversionMethod : null;
        var state = Expression.Parameter(typeof(TState));
        var value = Expression.Parameter(argumentType);
        var body = Expression.Invoke(accumulate, state, Expression.Convert(value, typeof(TValue), method));
        return Expression.Lambda(body, state, value);
    }
}
