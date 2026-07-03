namespace NQuery.Metadata;

// The AggregateDefinition produced by AggregateDefinition.Create. Resolving an argument type
// runs the author's binder to get the fold (or null / AggregateNotApplicableException when the
// type isn't supported). The fold's lambdas are left uncompiled; the emitter compiles them.
internal sealed class FoldAggregateDefinition : AggregateDefinition
{
    private readonly Func<Type, AggregateFold?> _binder;

    public FoldAggregateDefinition(string name, Func<Type, AggregateFold?> binder)
    {
        ThrowIfNull(name);
        ThrowIfNull(binder);

        Name = name;
        _binder = binder;
    }

    public override string Name { get; }

    internal override AggregateFold? CreateFold(Type argumentType)
    {
        try
        {
            return _binder(argumentType);
        }
        catch (AggregateNotApplicableException)
        {
            return null;
        }
    }
}

// A binder (see AggregateDefinition.Create) signals that it doesn't support an argument type
// either by returning null or by throwing this exception; the built-in aggregates throw it from
// their operator/conversion helpers so a deep failure unwinds without threading null back out.
internal sealed class AggregateNotApplicableException : Exception
{
}
