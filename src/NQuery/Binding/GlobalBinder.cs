using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class GlobalBinder : Binder
    {
        private readonly DataContext _dataContext;

        public GlobalBinder(Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, DataContext dataContext)
            : base(null, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics)
        {
            _dataContext = dataContext;
        }

        public override IEnumerable<Symbol> GetLocalSymbols()
        {
            return _dataContext.Tables.Cast<Symbol>()
                               .Concat(_dataContext.Functions)
                               .Concat(_dataContext.Aggregates)
                               .Concat(_dataContext.Variables);
        }

        public override IEnumerable<PropertySymbol> LookupProperties(Type type)
        {
            // TODO: Should we cache them to ensure object identity for property symbols?
            var propertyProvider = _dataContext.PropertyProviders.LookupValue(type);
            return propertyProvider == null
                       ? Enumerable.Empty<PropertySymbol>()
                       : propertyProvider.GetProperties(type);
        }

        public override IEnumerable<MethodSymbol> LookupMethods(Type type)
        {
            // TODO: Should we cache them to ensure object identity for method symbols?
            var methodProvider = _dataContext.MethodProviders.LookupValue(type);
            return methodProvider == null
                       ? Enumerable.Empty<MethodSymbol>()
                       : methodProvider.GetMethods(type);
        }
    }
}