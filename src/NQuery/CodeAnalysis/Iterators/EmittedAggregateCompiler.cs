using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq.Expressions;

using NQuery.CodeAnalysis.Algebra;

namespace NQuery.CodeAnalysis.Iterators;

// Compiles a whole stream-aggregate's folds into a factory for EmittedAggregates. Each fold's
// accumulator becomes a typed variable (state0, state1, ...); the three emitted delegates close
// over that same set of variables, so initializing/folding/reading never boxes the accumulators.
// Invoking the factory hoists a fresh set of variables into a new closure -- one per iterator.
internal static class EmittedAggregateCompiler
{
    public static Func<EmittedAggregates> Compile(ImmutableArray<LogicalAggregatedValue> aggregates, FrozenDictionary<ValueSlot, int> slotIndices, int outputOffset)
    {
        var buffer = Expression.Parameter(typeof(RowBuffer), "buffer");
        var target = Expression.Parameter(typeof(object[]), "target");

        var states = new ParameterExpression[aggregates.Length];
        var seedStatements = new Expression[aggregates.Length];
        var accumulateStatements = new Expression[aggregates.Length];
        var storeStatements = new Expression[aggregates.Length];

        for (var i = 0; i < aggregates.Length; i++)
        {
            var fold = aggregates[i].Fold;
            var stateType = fold.Seed.ReturnType;
            var valueType = fold.Accumulate.Parameters[1].Type;
            var state = Expression.Variable(stateType, "state" + i);
            states[i] = state;

            // state = seed()
            seedStatements[i] = Expression.Assign(state, Expression.Invoke(fold.Seed));

            // var arg = argument(buffer); if (arg is not null) state = accumulate(state, (TValue)arg)
            var argument = EmittedExpressionCompiler.CompileFunction(aggregates[i].Argument, slotIndices);
            var arg = Expression.Variable(typeof(object), "arg" + i);
            accumulateStatements[i] = Expression.Block(
                new[] { arg },
                Expression.Assign(arg, Expression.Invoke(Expression.Constant(argument), buffer)),
                Expression.IfThen(
                    Expression.ReferenceNotEqual(arg, Expression.Constant(null, typeof(object))),
                    Expression.Assign(state, Expression.Invoke(fold.Accumulate, state, Expression.Convert(arg, valueType)))));

            // target[outputOffset + i] = (object)result(state)
            storeStatements[i] = Expression.Assign(
                Expression.ArrayAccess(target, Expression.Constant(outputOffset + i)),
                Expression.Convert(Expression.Invoke(fold.Result, state), typeof(object)));
        }

        var initialize = Expression.Lambda<Action>(AsBody(seedStatements));
        var accumulate = Expression.Lambda<Action<RowBuffer>>(AsBody(accumulateStatements), buffer);
        var storeResults = Expression.Lambda<Action<object[]>>(AsBody(storeStatements), target);

        var factory = Expression.Lambda<Func<EmittedAggregates>>(
            Expression.Block(
                states,
                Expression.New(
                    typeof(EmittedAggregates).GetConstructors()[0],
                    initialize, accumulate, storeResults)));

        return factory.Compile();
    }

    private static Expression AsBody(Expression[] statements)
    {
        return statements.Length == 0 ? Expression.Empty() : Expression.Block(statements);
    }
}
