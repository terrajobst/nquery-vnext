using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class GlobalBinder : Binder
    {
        private readonly DataContext _dataContext;
        private readonly SymbolTable _localSymbols;

        private readonly Dictionary<Type, ImmutableArray<PropertySymbol>> _propertySymbols = new Dictionary<Type, ImmutableArray<PropertySymbol>>();
        private readonly Dictionary<Type, ImmutableArray<MethodSymbol>> _methodSymbols = new Dictionary<Type, ImmutableArray<MethodSymbol>>();

        public GlobalBinder(SharedBinderState sharedBinderState, DataContext dataContext)
            : base(sharedBinderState, null)
        {
            var symbols = dataContext.Tables.Cast<Symbol>()
                                     .Concat(dataContext.Functions)
                                     .Concat(dataContext.Aggregates)
                                     .Concat(dataContext.Variables);

            _dataContext = dataContext;
            _localSymbols = SymbolTable.Create(symbols);
        }

        public override SymbolTable LocalSymbols
        {
            get { return _localSymbols; }
        }

        public override IEnumerable<PropertySymbol> LookupProperties(Type type)
        {
            var propertyProvider = Lookup(_dataContext.PropertyProviders, type);
            if (propertyProvider == null)
                return Enumerable.Empty<PropertySymbol>();

            ImmutableArray<PropertySymbol> result;
            if (!_propertySymbols.TryGetValue(type, out result))
            {
                result = propertyProvider.GetProperties(type).ToImmutableArray();
                _propertySymbols.Add(type, result);
            }

            return result;
        }

        public override IEnumerable<MethodSymbol> LookupMethods(Type type)
        {
            var methodProvider = Lookup(_dataContext.MethodProviders, type);
            if (methodProvider == null)
                return Enumerable.Empty<MethodSymbol>();

            ImmutableArray<MethodSymbol> result;
            if (!_methodSymbols.TryGetValue(type, out result))
            {
                result = methodProvider.GetMethods(type).ToImmutableArray();
                _methodSymbols.Add(type, result);
            }

            return result;
        }

        public override IComparer LookupComparer(Type type)
        {
            var registeredComparer = Lookup(_dataContext.Comparers, type);
            if (registeredComparer != null)
                return registeredComparer;

            return type.IsComparable() ? Comparer.Default : null;
        }

        private static T Lookup<T>(IReadOnlyDictionary<Type, T> dictionary, Type key) where T: class
        {
            while (key != null)
            {
                T value;
                if (dictionary.TryGetValue(key, out value))
                    return value;

                key = key.BaseType;
            }

            return null;
        }
    }
}