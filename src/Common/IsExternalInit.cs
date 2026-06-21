#if NETFRAMEWORK

// Polyfill for System.Runtime.CompilerServices.IsExternalInit, which only exists on
// .NET 5+. NQuery also targets net481, so this provides the type there. The compiler
// requires it to emit any 'init'-only setter (e.g. positional records); the guard keeps
// it from colliding with the framework-provided type on net8.0.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit
{
}

#endif
