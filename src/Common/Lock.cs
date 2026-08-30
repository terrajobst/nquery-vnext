#if !NET9_0_OR_GREATER

namespace System.Threading;

// Polyfill of the Lock type introduced in .NET 9. The C# compiler recognizes it by name, so a
// `lock` statement over one of these takes the Lock path rather than the object path even though
// the type is declared here -- which is what keeps the call sites identical across target
// frameworks.
//
// Monitor is the whole implementation: the point of using Lock downlevel is that a gate is typed
// as a gate rather than as an object anybody could lock on, not that the locking gets faster. Only
// the members the codebase uses are here.
internal sealed class Lock
{
    public Scope EnterScope()
    {
        Enter();
        return new Scope(this);
    }

    // CS9216 fires because these hand a Lock to an API taking object, which is exactly the mistake
    // the warning exists to catch. Here it is the implementation rather than a mistake.
#pragma warning disable CS9216
    public void Enter()
    {
        Monitor.Enter(this);
    }

    public void Exit()
    {
        Monitor.Exit(this);
    }
#pragma warning restore CS9216

    public ref struct Scope
    {
        private readonly Lock _lock;

        internal Scope(Lock @lock)
        {
            _lock = @lock;
        }

        public void Dispose()
        {
            _lock.Exit();
        }
    }
}

#endif
