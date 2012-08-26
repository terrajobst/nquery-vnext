using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
        private IEnumerable<Symbol> LookupName(string name)
        {
            return Context.LookupSymbols(name, false);
        }

        private IEnumerable<VariableSymbol> LookupVariable(string name)
        {
            return Context.LookupSymbols(name, false).OfType<VariableSymbol>();
        }

        private IEnumerable<TableSymbol> LookupTable(string name)
        {
            return Context.LookupSymbols(name, false).OfType<TableSymbol>();
        }

        private IEnumerable<TableInstanceSymbol> LookupTableInstances()
        {
            return Context.LookupSymbols().OfType<TableInstanceSymbol>();
        }

        private IEnumerable<TableInstanceSymbol> LookupTableInstance(string name)
        {
            return Context.LookupSymbols(name, false).OfType<TableInstanceSymbol>();
        }

        private IEnumerable<PropertySymbol> LookupProperty(Type type, string name)
        {
            var propertyProvider = _dataContext.PropertyProviders.LookupValue(type);
            if (propertyProvider == null)
                return Enumerable.Empty<PropertySymbol>();

            return from p in propertyProvider.GetProperties(type)
                   where string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                   select p;
        }

        private IEnumerable<MethodSymbol> LookupMethod(Type type, string name, int parameterCount)
        {
            var methodProvider = _dataContext.MethodProviders.LookupValue(type);
            if (methodProvider == null)
                return Enumerable.Empty<MethodSymbol>();

            var methods = methodProvider.GetMethods(type);
            return LookupInvocable(methods, name, parameterCount);
        }

        private IEnumerable<FunctionSymbol> LookupFunction(string name, int parameterCount)
        {
            return LookupInvocable(_dataContext.Functions, name, parameterCount);
        }

        private static IEnumerable<T> LookupInvocable<T>(IEnumerable<T> invocables, string name, int parameterCount)
            where T : InvocableSymbol
        {
            return from m in invocables
                   where m.Parameters.Count == parameterCount &&
                         string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)
                   select m;
        }

    }
}