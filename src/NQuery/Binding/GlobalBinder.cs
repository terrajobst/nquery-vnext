using System.Collections;
using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class GlobalBinder : Binder
    {
        private readonly DataContext _dataContext;

        private readonly Dictionary<Type, ImmutableArray<PropertySymbol>> _propertySymbols = new();
        private readonly Dictionary<Type, ImmutableArray<MethodSymbol>> _methodSymbols = new();

        public GlobalBinder(SharedBinderState sharedBinderState, DataContext dataContext)
            : base(sharedBinderState, null)
        {
            var symbols = dataContext.Tables.Cast<Symbol>()
                                     .Concat(dataContext.Functions)
                                     .Concat(dataContext.Aggregates)
                                     .Concat(dataContext.Variables);

            _dataContext = dataContext;
            LocalSymbols = SymbolTable.Create(symbols);
        }

        public override SymbolTable LocalSymbols { get; }

        public override IEnumerable<PropertySymbol> LookupProperties(Type type)
        {
            var propertyProvider = Lookup(_dataContext.PropertyProviders, type);
            if (propertyProvider is null)
                return Enumerable.Empty<PropertySymbol>();

            if (!_propertySymbols.TryGetValue(type, out var result))
            {
                result = propertyProvider.GetProperties(type).ToImmutableArray();
                _propertySymbols.Add(type, result);
            }

            return result;
        }

        public override IEnumerable<MethodSymbol> LookupMethods(Type type)
        {
            var methodProvider = Lookup(_dataContext.MethodProviders, type);
            if (methodProvider is null)
                return Enumerable.Empty<MethodSymbol>();

            if (!_methodSymbols.TryGetValue(type, out var result))
            {
                result = methodProvider.GetMethods(type).ToImmutableArray();
                _methodSymbols.Add(type, result);
            }

            return result;
        }

        public override IComparer LookupComparer(Type type)
        {
            var registeredComparer = Lookup(_dataContext.Comparers, type);
            if (registeredComparer is not null)
                return registeredComparer;

            return type.IsComparable() ? Comparer.Default : null;
        }

        private static T Lookup<T>(IReadOnlyDictionary<Type, T> dictionary, Type key) where T : class
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
}