using System.Collections;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class GlobalBinder : Binder
{
    private readonly Catalog _catalog;

    private readonly Dictionary<Type, ImmutableArray<PropertySymbol>> _propertySymbols = new();
    private readonly Dictionary<Type, ImmutableArray<MethodSymbol>> _methodSymbols = new();

    public GlobalBinder(SharedBinderState sharedBinderState, Catalog catalog)
        : base(sharedBinderState, null)
    {
        var symbols = catalog.Tables.Select(t => (Symbol)new SchemaTableSymbol(t))
                                 .Concat(catalog.Functions.Select(f => new FunctionSymbol(f)))
                                 .Concat(catalog.Aggregates.Select(a => new AggregateSymbol(a)))
                                 .Concat(catalog.Variables.Select(v => new VariableSymbol(v)));

        _catalog = catalog;
        LocalSymbols = SymbolTable.Create(symbols);
    }

    public override SymbolTable LocalSymbols { get; }

    public override IEnumerable<PropertySymbol> LookupProperties(Type type)
    {
        var propertyProvider = Lookup(_catalog.PropertyProviders, type);
        if (propertyProvider is null)
            return [];

        if (!_propertySymbols.TryGetValue(type, out var result))
        {
            result = [.. propertyProvider.GetProperties(type).Select(p => new PropertySymbol(p))];
            _propertySymbols.Add(type, result);
        }

        return result;
    }

    public override IEnumerable<MethodSymbol> LookupMethods(Type type)
    {
        var methodProvider = Lookup(_catalog.MethodProviders, type);
        if (methodProvider is null)
            return [];

        if (!_methodSymbols.TryGetValue(type, out var result))
        {
            result = [.. methodProvider.GetMethods(type).Select(m => new MethodSymbol(m))];
            _methodSymbols.Add(type, result);
        }

        return result;
    }

    public override IComparer? LookupComparer(Type type)
    {
        var registeredComparer = Lookup(_catalog.Comparers, type);
        if (registeredComparer is not null)
            return CatalogComparer.EnsureTyped(type, registeredComparer);

        return type.IsComparable() ? Comparer.Default : null;
    }

    private static T? Lookup<T>(IReadOnlyDictionary<Type, T> dictionary, Type? key) where T : class
    {
        while (key is not null)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            key = key.BaseType;
        }

        return null;
    }
}
