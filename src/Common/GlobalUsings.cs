// Imported into every project via Directory.Build.targets (Common\*.cs). The static imports
// let call sites use the bare ThrowIfNull(...) / ThrowIfNullOrEmpty(...) forms instead of
// qualifying them with the exception type.

global using static System.ArgumentException;

#if NETFRAMEWORK
// .NET Framework's ArgumentNullException/ArgumentOutOfRangeException lack the ThrowIf* guard
// helpers, so we import the types that supply them as extension members to keep the unqualified
// call form working.
global using static NQuery.ArgumentNullExceptionExtensions;
global using static NQuery.ArgumentOutOfRangeExceptionExtensions;
#else
global using static System.ArgumentNullException;
global using static System.ArgumentOutOfRangeException;
#endif
