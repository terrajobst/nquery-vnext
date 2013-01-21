using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class GlobalBinder : Binder
    {
        private readonly DataContext _dataContext;
        private readonly SymbolTable _localSymbols;

        public GlobalBinder(Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory, DataContext dataContext)
            : base(null, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
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
            // TODO: Should we cache them to ensure object identity for property symbols?
            var propertyProvider = Lookup(_dataContext.PropertyProviders, type);
            return propertyProvider == null
                       ? Enumerable.Empty<PropertySymbol>()
                       : propertyProvider.GetProperties(type);
        }

        public override IEnumerable<MethodSymbol> LookupMethods(Type type)
        {
            // TODO: Should we cache them to ensure object identity for method symbols?
            var methodProvider = Lookup(_dataContext.MethodProviders, type);
            return methodProvider == null
                       ? Enumerable.Empty<MethodSymbol>()
                       : methodProvider.GetMethods(type);
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