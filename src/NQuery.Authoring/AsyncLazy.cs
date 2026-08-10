using System.Diagnostics.CodeAnalysis;

namespace NQuery.Authoring;

// A value produced either synchronously or asynchronously, computed at most once.
//
// Document hands out syntax trees, compilations and semantic models through a sync and an async
// entry point, and both have to yield the *same* instance: symbols are compared by reference (see
// SymbolSearchService), so a document that produced two semantic models would break find-usages in
// a way that reproduces roughly never. Caching the Task<T> and publishing it with Interlocked
// can't do that once a sync path sits beside the async one -- the sync caller computes locally,
// loses the race, and then either returns a foreign instance or blocks on the task it lost to.
//
// So the value is cached, not the task. The lock is held across the factory, which is the
// ExecutionAndPublication behavior of Lazy<T>; the factory never awaits, so it cannot deadlock.
internal sealed class AsyncLazy<T>
    where T : class
{
    private readonly Func<CancellationToken, T> _valueFactory;
    private readonly object _gate = new();

    private T? _value;

    public AsyncLazy(Func<CancellationToken, T> valueFactory)
    {
        _valueFactory = valueFactory;
    }

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = Volatile.Read(ref _value);
        return value is not null;
    }

    public T GetValue(CancellationToken cancellationToken)
    {
        var value = Volatile.Read(ref _value);
        if (value is not null)
            return value;

        lock (_gate)
        {
            var current = _value;

            if (current is null)
            {
                current = _valueFactory(cancellationToken);
                Volatile.Write(ref _value, current);
            }

            return current;
        }
    }

    public Task<T> GetValueAsync(CancellationToken cancellationToken)
    {
        // The work is CPU-bound throughout -- there is no I/O below a document -- so async here is
        // purely offloading. A host that wants it on its own thread calls GetValue() instead.
        return TryGetValue(out var value)
                ? Task.FromResult(value)
                : Task.Run(() => GetValue(cancellationToken), cancellationToken);
    }
}
