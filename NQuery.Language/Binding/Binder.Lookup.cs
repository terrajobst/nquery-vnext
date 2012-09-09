using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
        private IEnumerable<Symbol> LookupSymbols()
        {
            return _bindingContext.LookupSymbols();
        }

        private IEnumerable<Symbol> LookupSymbols(SyntaxToken token)
        {
            return LookupSymbols().Where(s => token.Matches(s.Name));
        }

        private IEnumerable<Symbol> LookupName(SyntaxToken name)
        {
            return LookupSymbols(name);
        }

        private IEnumerable<VariableSymbol> LookupVariable(SyntaxToken name)
        {
            return LookupSymbols(name).OfType<VariableSymbol>();
        }

        private IEnumerable<TableSymbol> LookupTable(SyntaxToken name)
        {
            return LookupSymbols(name).OfType<TableSymbol>();
        }

        private IEnumerable<TableInstanceSymbol> LookupTableInstances()
        {
            return LookupSymbols().OfType<TableInstanceSymbol>();
        }

        private IEnumerable<TableInstanceSymbol> LookupTableInstance(SyntaxToken name)
        {
            return LookupSymbols(name).OfType<TableInstanceSymbol>();
        }

        private IEnumerable<PropertySymbol> LookupProperty(Type type, SyntaxToken name)
        {
            var propertyProvider = _dataContext.PropertyProviders.LookupValue(type);
            return propertyProvider == null
                       ? Enumerable.Empty<PropertySymbol>()
                       : propertyProvider.GetProperties(type).Where(p => name.Matches(p.Name));
        }

        private IEnumerable<MethodSymbol> LookupMethod(Type type, SyntaxToken name, int parameterCount)
        {
            var methodProvider = _dataContext.MethodProviders.LookupValue(type);
            if (methodProvider == null)
                return Enumerable.Empty<MethodSymbol>();

            var methods = methodProvider.GetMethods(type);
            return LookupInvocable(methods, name, parameterCount);
        }

        private IEnumerable<FunctionSymbol> LookupFunction(SyntaxToken name, int parameterCount)
        {
            return LookupInvocable(_dataContext.Functions, name, parameterCount);
        }

        private static IEnumerable<T> LookupInvocable<T>(IEnumerable<T> invocables, SyntaxToken name, int parameterCount)
            where T : InvocableSymbol
        {
            return from m in invocables
                   where m.Parameters.Count == parameterCount &&
                         name.Matches(m.Name)
                   select m;
        }
    }
}