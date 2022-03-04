using NQuery.Binding;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery
{
    public sealed class SemanticModel
    {
        private readonly BindingResult _bindingResult;

        internal SemanticModel(Compilation compilation, BindingResult bindingResult)
        {
            Compilation = compilation;
            _bindingResult = bindingResult;
        }

        public Compilation Compilation { get; }

        public SyntaxTree SyntaxTree => Compilation.SyntaxTree;

        public Conversion ClassifyConversion(Type sourceType, Type targetType)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            return Conversion.Classify(sourceType, targetType);
        }

        public TableInstanceSymbol GetTableInstance(WildcardSelectColumnSyntax selectColumn)
        {
            if (selectColumn is null)
                throw new ArgumentNullException(nameof(selectColumn));

            var boundExpression = _bindingResult.GetBoundNode(selectColumn) as BoundWildcardSelectColumn;
            return boundExpression?.Table;
        }

        public IEnumerable<TableColumnInstanceSymbol> GetColumnInstances(WildcardSelectColumnSyntax selectColumn)
        {
            if (selectColumn is null)
                throw new ArgumentNullException(nameof(selectColumn));

            var boundExpression = _bindingResult.GetBoundNode(selectColumn) as BoundWildcardSelectColumn;
            return boundExpression?.TableColumns ?? Enumerable.Empty<TableColumnInstanceSymbol>();
        }

        public IEnumerable<QueryColumnInstanceSymbol> GetOutputColumns(QuerySyntax query)
        {
            if (query is null)
                throw new ArgumentNullException(nameof(query));

            var boundQuery = _bindingResult.GetBoundNode(query) as BoundQuery;
            return boundQuery?.OutputColumns ?? Enumerable.Empty<QueryColumnInstanceSymbol>();
        }

        public QueryColumnInstanceSymbol GetSymbol(OrderByColumnSyntax orderByColumn)
        {
            if (orderByColumn is null)
                throw new ArgumentNullException(nameof(orderByColumn));

            var boundOrderByColumn = _bindingResult.GetBoundNode(orderByColumn) as BoundOrderByColumn;
            return boundOrderByColumn?.QueryColumn;
        }

        public Symbol GetSymbol(ExpressionSyntax expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var boundExpression = GetBoundExpression(expression);
            return boundExpression is null ? null : GetSymbol(boundExpression);
        }

        private static Symbol GetSymbol(BoundExpression expression)
        {
            switch (expression.Kind)
            {
                case BoundNodeKind.TableExpression:
                    return GetSymbol((BoundTableExpression)expression);
                case BoundNodeKind.ColumnExpression:
                    return GetSymbol((BoundColumnExpression)expression);
                case BoundNodeKind.VariableExpression:
                    return GetSymbol((BoundVariableExpression)expression);
                case BoundNodeKind.FunctionInvocationExpression:
                    return GetSymbol((BoundFunctionInvocationExpression)expression);
                case BoundNodeKind.AggregateExpression:
                    return GetSymbol((BoundAggregateExpression)expression);
                case BoundNodeKind.PropertyAccessExpression:
                    return GetSymbol((BoundPropertyAccessExpression)expression);
                case BoundNodeKind.MethodInvocationExpression:
                    return GetSymbol(((BoundMethodInvocationExpression)expression));
                default:
                    return null;
            }
        }

        private static Symbol GetSymbol(BoundTableExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundColumnExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundVariableExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundFunctionInvocationExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundAggregateExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundPropertyAccessExpression expression)
        {
            return expression.Symbol;
        }

        private static Symbol GetSymbol(BoundMethodInvocationExpression expression)
        {
            return expression.Symbol;
        }

        public Type GetExpressionType(ExpressionSyntax expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var boundExpression = GetBoundExpression(expression);
            return boundExpression?.Type;
        }

        public Conversion GetConversion(CastExpressionSyntax expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var boundExpression = GetBoundExpression(expression) as BoundConversionExpression;
            return boundExpression?.Conversion;
        }

        private BoundExpression GetBoundExpression(ExpressionSyntax expression)
        {
            return _bindingResult.GetBoundNode(expression) as BoundExpression;
        }

        public IEnumerable<TableInstanceSymbol> GetDeclaredSymbols(TableReferenceSyntax tableReference)
        {
            if (tableReference is null)
                throw new ArgumentNullException(nameof(tableReference));

            var result = _bindingResult.GetBoundNode(tableReference) as BoundRelation;
            return result?.GetDeclaredTableInstances().AsEnumerable();
        }

        public CommonTableExpressionSymbol GetDeclaredSymbol(CommonTableExpressionSyntax commonTableExpression)
        {
            if (commonTableExpression is null)
                throw new ArgumentNullException(nameof(commonTableExpression));

            var result = _bindingResult.GetBoundNode(commonTableExpression) as BoundCommonTableExpression;
            return result?.TableSymbol;
        }

        public ColumnSymbol GetDeclaredSymbol(CommonTableExpressionColumnNameSyntax commonTableExpressionColumnName)
        {
            if (commonTableExpressionColumnName is null)
                throw new ArgumentNullException(nameof(commonTableExpressionColumnName));

            var columnList = commonTableExpressionColumnName.Parent as CommonTableExpressionColumnNameListSyntax;
            if (columnList is null)
                return null;

            var commonTableExpression = columnList.Parent as CommonTableExpressionSyntax;
            if (commonTableExpression is null)
                return null;

            var symbol = GetDeclaredSymbol(commonTableExpression);
            if (symbol is null)
                return null;

            var index = 0;

            foreach (var columnName in columnList.ColumnNames)
            {
                if (columnName == commonTableExpressionColumnName)
                {
                    if (index < symbol.Columns.Length)
                        return symbol.Columns[index];
                }
                index++;
            }

            return null;
        }

        public TableInstanceSymbol GetDeclaredSymbol(NamedTableReferenceSyntax tableReference)
        {
            if (tableReference is null)
                throw new ArgumentNullException(nameof(tableReference));

            var result = _bindingResult.GetBoundNode(tableReference) as BoundTableRelation;
            return result?.TableInstance;
        }

        public TableInstanceSymbol GetDeclaredSymbol(DerivedTableReferenceSyntax tableReference)
        {
            if (tableReference is null)
                throw new ArgumentNullException(nameof(tableReference));

            var result = _bindingResult.GetBoundNode(tableReference) as BoundDerivedTableRelation;
            return result?.TableInstance;
        }

        public QueryColumnInstanceSymbol GetDeclaredSymbol(ExpressionSelectColumnSyntax selectColumn)
        {
            if (selectColumn is null)
                throw new ArgumentNullException(nameof(selectColumn));

            var result = _bindingResult.GetBoundNode(selectColumn) as BoundSelectColumn;
            return result?.Column;
        }

        public IEnumerable<QueryColumnInstanceSymbol> GetDeclaredSymbols(WildcardSelectColumnSyntax selectColumn)
        {
            if (selectColumn is null)
                throw new ArgumentNullException(nameof(selectColumn));

            var boundExpression = _bindingResult.GetBoundNode(selectColumn) as BoundWildcardSelectColumn;
            return boundExpression?.QueryColumns ?? Enumerable.Empty<QueryColumnInstanceSymbol>();
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _bindingResult.Diagnostics;
        }

        public IEnumerable<Symbol> LookupSymbols(int position)
        {
            var node = FindClosestNodeWithBinder(_bindingResult.Root, position);
            var binder = node is null ? null : _bindingResult.GetBinder(node);
            return binder is null
                       ? Enumerable.Empty<Symbol>()
                       : LookupSymbols(binder);
        }

        private static IEnumerable<Symbol> LookupSymbols(Binder binder)
        {
            // NOTE: We want to only show the *available* symbols. That means, we need to
            //       hide symbols from the parent binder that have same name as the ones
            //       from a nested binder.
            //
            //       We do this by simply recording which names we've already seen.
            //       Please note that we *do* want to see duplicate names within the
            //       *same* binder.

            var allNames = new HashSet<string>();

            while (binder is not null)
            {
                var localNames = new HashSet<string>();
                var localSymbols = binder.LocalSymbols
                                         .Where(s => !string.IsNullOrEmpty(s.Name));

                foreach (var symbol in localSymbols)
                {
                    if (!allNames.Contains(symbol.Name))
                    {
                        localNames.Add(symbol.Name);
                        yield return symbol;
                    }
                }

                allNames.UnionWith(localNames);
                binder = binder.Parent;
            }
        }

        private SyntaxNode FindClosestNodeWithBinder(SyntaxNode root, int position)
        {
            var token = root.FindTokenContext(position);
            return (from n in token.Parent.AncestorsAndSelf()
                    let bc = _bindingResult.GetBinder(n)
                    where bc is not null
                    select n).FirstOrDefault();
        }

        public IEnumerable<MethodSymbol> LookupMethods(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return _bindingResult.RootBinder.LookupMethods(type);
        }

        public IEnumerable<PropertySymbol> LookupProperties(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return _bindingResult.RootBinder.LookupProperties(type);
        }
    }
}