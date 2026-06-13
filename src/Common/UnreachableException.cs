#if NETFRAMEWORK

#nullable enable

// Polyfill for System.Diagnostics.UnreachableException, which only exists on
// .NET 7+. NQuery also targets net481, so this provides the type there. The
// guard keeps it from colliding with the framework-provided type on net8.0.
namespace System.Diagnostics
{
    internal sealed class UnreachableException : Exception
    {
        public UnreachableException()
            : base("The program executed an instruction that was thought to be unreachable.")
        {
        }

        public UnreachableException(string? message)
            : base(message)
        {
        }

        public UnreachableException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

#endif
