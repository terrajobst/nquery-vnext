using System.Collections.Immutable;

namespace NQuery.Symbols;

internal interface IInvocableSymbol
{
    ImmutableArray<ParameterSymbol> Parameters { get; }

    Type ReturnType { get; }
}
