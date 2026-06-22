using System.Linq.Expressions;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

// Describes a custom aggregate as a fold over the input values: a seed that produces the
// initial accumulator, an accumulate step folding one value into the accumulator, and a
// result selector projecting the final accumulator to the aggregate's value.
//
//   Seed:       () => TAccumulate
//   Accumulate: (TAccumulate, TValue) => TAccumulate
//   Result:     (TAccumulate) => TResult
//
// The accumulate step is only invoked for non-null values, and the result selector's
// return type is the aggregate's value type. The lambdas are ordinary expression trees,
// so authors compose them with System.Linq.Expressions; nothing NQuery-specific is required.
public sealed class AggregateFold
{
    public AggregateFold(LambdaExpression seed, LambdaExpression accumulate, LambdaExpression result)
    {
        ThrowIfNull(seed);
        ThrowIfNull(accumulate);
        ThrowIfNull(result);

        Seed = seed;
        Accumulate = accumulate;
        Result = result;
    }

    public LambdaExpression Seed { get; }

    public LambdaExpression Accumulate { get; }

    public LambdaExpression Result { get; }

    // The aggregate's value type. The result lambda is nullable-lifted (it yields null over an
    // empty group), but NQuery types are non-nullable, so the underlying type is the value type.
    public Type ReturnType => Result.ReturnType.GetNonNullableType();
}
