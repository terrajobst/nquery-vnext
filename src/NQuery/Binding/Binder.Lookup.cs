using System.Collections;
using System.Collections.Immutable;

using NQuery.Symbols;
using NQuery.Symbols.Aggregation;

namespace NQuery.Binding
{
    partial class Binder
    {
        public virtual SymbolTable LocalSymbols
        {
            get { return SymbolTable.Empty; }
        }

        private static Type LookupType(SyntaxToken name)
        {
            var normalizedName = name.IsQuotedIdentifier()
                                     ? name.ValueText
                                     : name.ValueText.ToUpper();

            switch (normalizedName)
            {
                case @"BOOL":
                case @"BOOLEAN":
                    return typeof(bool);
                case @"BYTE":
                    return typeof(byte);
                case @"SBYTE":
                    return typeof(sbyte);
                case @"CHAR":
                    return typeof(char);
                case @"SHORT":
                case @"INT16":
                    return typeof(short);
                case @"USHORT":
                case @"UINT16":
                    return typeof(ushort);
                case @"INT":
                case @"INT32":
                    return typeof(int);
                case @"UINT":
                case @"UINT32":
                    return typeof(uint);
                case @"LONG":
                case @"INT64":
                    return typeof(long);
                case @"ULONG":
                case @"UINT64":
                    return typeof(ulong);
                case @"FLOAT":
                case @"SINGLE":
                    return typeof(float);
                case @"DOUBLE":
                    return typeof(double);
                case @"DECIMAL":
                    return typeof(decimal);
                case @"STRING":
                    return typeof(string);
                case @"OBJECT":
                    return typeof(object);
                default:
                    return null;
            }
        }

        private IEnumerable<Symbol> LookupSymbols(SyntaxToken name, Func<Symbol, bool> filter)
        {
            var text = name.ValueText;
            var isCaseSensitive = name.IsQuotedIdentifier();

            IEnumerable<Symbol> result;
            var binder = this;
            do
            {
                result = binder.LocalSymbols.Lookup(text, isCaseSensitive).Where(filter);
                binder = binder.Parent;
            } while (!result.Any() && binder is not null);

            return result;
        }

        private IEnumerable<T> LookupSymbols<T>(SyntaxToken name)
            where T : Symbol
        {
            return LookupSymbols(name, s => s is T).OfType<T>();
        }

        private IEnumerable<Symbol> LookupColumnTableOrVariable(SyntaxToken name)
        {
            return LookupSymbols(name, s => s is TableColumnInstanceSymbol ||
                                            s is TableInstanceSymbol ||
                                            s is VariableSymbol);
        }

        private IEnumerable<QueryColumnInstanceSymbol> LookupQueryColumn(SyntaxToken name)
        {
            return LookupSymbols<QueryColumnInstanceSymbol>(name);
        }

        private IEnumerable<VariableSymbol> LookupVariable(SyntaxToken name)
        {
            return LookupSymbols<VariableSymbol>(name);
        }

        private IEnumerable<TableSymbol> LookupTable(SyntaxToken name)
        {
            return LookupSymbols<TableSymbol>(name);
        }

        private IEnumerable<TableInstanceSymbol> LookupTableInstances()
        {
            return QueryState?.IntroducedTables.Keys ?? Enumerable.Empty<TableInstanceSymbol>();
        }

        private IEnumerable<TableInstanceSymbol> LookupTableInstance(SyntaxToken name)
        {
            return LookupSymbols<TableInstanceSymbol>(name);
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

        private OverloadResolutionResult<MethodSymbolSignature> LookupMethod(Type type, SyntaxToken name, ImmutableArray<Type> argumentTypes)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            var signatures = from m in LookupMethod(type, name)
                             select new MethodSymbolSignature(m);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private OverloadResolutionResult<FunctionSymbolSignature> LookupFunction(SyntaxToken name, ImmutableArray<Type> argumentTypes)
        {
            var signatures = from f in LookupSymbols<FunctionSymbol>(name)
                             where name.Matches(f.Name)
                             select new FunctionSymbolSignature(f);
            return OverloadResolution.Perform(signatures, argumentTypes);
        }

        private IEnumerable<FunctionSymbol> LookupFunctionWithSingleParameter(SyntaxToken name)
        {
            return from f in LookupSymbols<FunctionSymbol>(name)
                   where f.Parameters.Length == 1
                   select f;
        }

        private IEnumerable<AggregateSymbol> LookupAggregate(SyntaxToken name)
        {
            return LookupSymbols<AggregateSymbol>(name);
        }

        public virtual IComparer LookupComparer(Type type)
        {
            return Parent.LookupComparer(type);
        }
    }
}