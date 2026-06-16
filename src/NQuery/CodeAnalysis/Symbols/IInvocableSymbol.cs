using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Symbols;

internal interface IInvocableSymbol
{
    ImmutableArray<ParameterSymbol> Parameters { get; }

    Type ReturnType { get; }
}
