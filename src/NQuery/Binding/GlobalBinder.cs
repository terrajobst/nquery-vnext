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

        public override IEnumerable<PropertySymbol> LookupProperty(Type type, SyntaxToken name)
        {
            var propertyProvider = _dataContext.PropertyProviders.LookupValue(type);
            return propertyProvider == null
                       ? Enumerable.Empty<PropertySymbol>()
                       : propertyProvider.GetProperties(type).Where(p => name.Matches(p.Name));
        }

        public override IEnumerable<MethodSymbol> LookupMethod(Type type, SyntaxToken name)
        {
            var methodProvider = _dataContext.MethodProviders.LookupValue(type);
            return methodProvider == null
                       ? Enumerable.Empty<MethodSymbol>()
                       : methodProvider.GetMethods(type).Where(m => name.Matches(m.Name));
        }
    }
}