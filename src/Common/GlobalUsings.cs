// Imported into every project via Directory.Build.targets (Common\*.cs). The static imports
// let call sites use the bare ThrowIfNull(...) / ThrowIfNullOrEmpty(...) forms instead of
// qualifying them with the exception type.

global using static System.ArgumentException;

#if NETFRAMEWORK
// .NET Framework's ArgumentNullException has no ThrowIfNull, so we import the type that
// supplies it as an extension member to keep the unqualified call form working.
global using static NQuery.ArgumentNullExceptionExtensions;
#else
global using static System.ArgumentNullException;
#endif
