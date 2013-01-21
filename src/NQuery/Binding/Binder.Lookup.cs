using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    partial class Binder
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

        public virtual IEnumerable<Symbol> GetLocalSymbols()
        {
            return Enumerable.Empty<Symbol>();
        }

        private IEnumerable<Symbol> LookupSymbols(SyntaxToken name)
        {
            IEnumerable<Symbol> result;
            var binder = this;
            do
            {
                result = binder.GetLocalSymbols().Where(s => name.Matches(s.Name));
                binder = binder.Parent;
            } while (!result.Any() && binder != null);

            return result;
        }

        private IEnumerable<Symbol> LookupColumnTableOrVariable(SyntaxToken name)
        {
            return LookupSymbols(name).Where(s => s is ColumnInstanceSymbol ||
                                                  s is TableInstanceSymbol ||
                                                  s is VariableSymbol);
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
            return GetLocalSymbols().OfType<TableInstanceSymbol>();
        }

        private IEnumerable<TableInstanceSymbol> LookupTableInstance(SyntaxToken name)
        {
            return LookupSymbols(name).OfType<TableInstanceSymbol>();
        }

        public virtual IEnumerable<PropertySymbol> LookupProperties(Type type)
        {
            return Parent.LookupProperties(type);
        }

        private IEnumerable<PropertySymbol> LookupProperty(Type type, SyntaxToken name)
        {
            return LookupProperties(type).Where(p => name.Matches(p.Name));
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

        public virtual IEnumerable<MethodSymbol> LookupMethods(Type type)
        {
            return Parent.LookupMethods(type);
        }

        private IEnumerable<MethodSymbol> LookupMethod(Type type, SyntaxToken name)
        {
            return LookupMethods(type).Where(m => name.Matches(m.Name));
        }

        public OverloadResolutionResult<MethodSymbolSignature> LookupMethod(Type type, SyntaxToken name, IList<Type> argumentTypes)
        {
            var signatures = from m in LookupMethod(type, name)
                             select new MethodSymbolSignature(m);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        public OverloadResolutionResult<FunctionSymbolSignature> LookupFunction(SyntaxToken name, IList<Type> argumentTypes)
        {
            var signatures = from f in LookupSymbols(name).OfType<FunctionSymbol>()
                             where name.Matches(f.Name)
                             select new FunctionSymbolSignature(f);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private IEnumerable<FunctionSymbol> LookupFunctionWithSingleParameter(SyntaxToken name)
        {
            return from f in LookupSymbols(name).OfType<FunctionSymbol>()
                   where f.Parameters.Count == 1
                   select f;
        }

        public IEnumerable<AggregateSymbol> LookupAggregate(SyntaxToken name)
        {
            return LookupSymbols(name).OfType<AggregateSymbol>();
        }
    }
}