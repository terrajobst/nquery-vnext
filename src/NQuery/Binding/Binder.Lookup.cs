using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed partial class Binder
    {
        private static Type LookupType(SyntaxToken name)
        {
            var normalizedName = name.IsQuotedIdentifier()
                                     ? name.ValueText
                                     : name.ValueText.ToUpper();

            switch (normalizedName)
            {
                case "BOOL":
                case "BOOLEAN":
                    return typeof(bool);
                case "BYTE":
                    return typeof(byte);
                case "SBYTE":
                    return typeof(sbyte);
                case "CHAR":
                    return typeof(char);
                case "SHORT":
                case "INT16":
                    return typeof(short);
                case "USHORT":
                case "UINT16":
                    return typeof(ushort);
                case "INT":
                case "INT32":
                    return typeof(int);
                case "UINT":
                case "UINT32":
                    return typeof(uint);
                case "LONG":
                case "INT64":
                    return typeof(long);
                case "ULONG":
                case "UINT64":
                    return typeof(ulong);
                case "FLOAT":
                case "SINGLE":
                    return typeof(float);
                case "DOUBLE":
                    return typeof(double);
                case "DECIMAL":
                    return typeof(decimal);
                case "STRING":
                    return typeof(string);
                case "OBJECT":
                    return typeof(object);
                default:
                    return null;
            }
        }

        private IEnumerable<Symbol> LookupSymbols()
        {
            return _bindingContext.LookupSymbols();
        }

        private IEnumerable<Symbol> LookupSymbols(SyntaxToken token)
        {
            return LookupSymbols().Where(s => token.Matches(s.Name));
        }

        private IEnumerable<Symbol> LookupColumnTableOrVariable(SyntaxToken name)
        {
            var columns = LookupTableInstances().SelectMany(t => t.ColumnInstances).Where(c => name.Matches(c.Name));
            var symbols = LookupSymbols(name).Where(s => s is TableInstanceSymbol || s is VariableSymbol);
            return columns.Concat(symbols);
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

        private static OverloadResolutionResult<UnaryOperatorSignature> LookupUnaryOperator(UnaryOperatorKind operatorKind, BoundExpression expression)
        {
            return UnaryOperator.Resolve(operatorKind, expression.Type);
        }

        private static OverloadResolutionResult<BinaryOperatorSignature> LookupBinaryOperator(BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right)
        {
            return LookupBinaryOperator(operatorKind, left.Type, right.Type);
        }

        private static OverloadResolutionResult<BinaryOperatorSignature> LookupBinaryOperator(BinaryOperatorKind operatorKind, Type left, Type right)
        {
            return BinaryOperator.Resolve(operatorKind, left, right);
        }

        private IEnumerable<MethodSymbol> LookupMethod(Type type, SyntaxToken name)
        {
            var methodProvider = _dataContext.MethodProviders.LookupValue(type);
            if (methodProvider == null)
                return Enumerable.Empty<MethodSymbol>();

            return from m in methodProvider.GetMethods(type)
                   where name.Matches(m.Name)
                   select m;
        }

        private OverloadResolutionResult<MethodSymbolSignature> LookupMethod(Type type, SyntaxToken name, IList<Type> argumentTypes)
        {
            var signatures = from m in LookupMethod(type, name)
                             select new MethodSymbolSignature(m);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private OverloadResolutionResult<FunctionSymbolSignature> LookupFunction(SyntaxToken name, IList<Type> argumentTypes)
        {
            var signatures = from f in _dataContext.Functions
                             where name.Matches(f.Name)
                             select new FunctionSymbolSignature(f);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private IEnumerable<FunctionSymbol> LookupFunctionWithSingleParameter(SyntaxToken name)
        {
            return from f in _dataContext.Functions
                   where f.Parameters.Count == 1 && name.Matches(f.Name)
                   select f;
        }

        private IEnumerable<AggregateSymbol> LookupAggregate(SyntaxToken name)
        {
            return LookupSymbols(name).OfType<AggregateSymbol>();
        }
    }
}