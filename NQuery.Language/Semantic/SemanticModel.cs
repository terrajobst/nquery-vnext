using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace NQuery.Language.Semantic
{
    public sealed class SemanticModel
    {
        private readonly Compilation _compilation;
        private readonly BindingResult _bindingResult;

        internal SemanticModel(Compilation compilation, BindingResult bindingResult)
        {
            _compilation = compilation;
            _bindingResult = bindingResult;
        }

        public Compilation Compilation
        {
            get { return _compilation; }
        }

        public Symbol GetSymbol(ExpressionSyntax expression)
        {
            var boundExpression = GetBoundExpression(expression);
            return boundExpression == null ? null : boundExpression.Symbol;
        }

        private BoundExpression GetBoundExpression(ExpressionSyntax expression)
        {
            return _bindingResult.GetBoundNode(expression) as BoundExpression;
        }

        public TableInstanceSymbol GetDeclaredSymbol(TableReferenceSyntax tableReference)
        {
            var result = _bindingResult.GetBoundNode(tableReference) as BoundNamedTableReference;
            return result == null ? null : result.TableInstance;
        }

        public Symbol GetDeclaredSymbol(DerivedTableReferenceSyntax tableReference)
        {
            var result = _bindingResult.GetBoundNode(tableReference) as BoundDerivedTableReference;
            return result == null ? null : result.TableInstance;
        }

        public IEnumerable<Diagnostic> GetDiagnostics()
        {
            return _bindingResult.Diagnostics;
        }

        public IEnumerable<Symbol> LookupSymbols(int position)
        {
            var node = FindClosestNodeWithBindingContext(_bindingResult.Root, position, 0);
            var bindingContext = node == null ? null : _bindingResult.GetBindingContext(node);
            return bindingContext != null ? bindingContext.LookupSymbols() : Enumerable.Empty<Symbol>();
        }

        private SyntaxNode FindClosestNodeWithBindingContext(SyntaxNode root, int position, int lastPosition)
        {
            foreach (var nodeOrToken in root.GetChildren())
            {
                if (lastPosition <= position && position < nodeOrToken.Span.End)
                {
                    if (nodeOrToken.IsToken)
                        return null;

                    var node = nodeOrToken.AsNode();
                    var result = FindClosestNodeWithBindingContext(node, position, lastPosition);
                    if (result != null)
                        return result;

                    if (_bindingResult.GetBindingContext(node) != null)
                        return node;
                }

                lastPosition = nodeOrToken.Span.End;
            }

            return null;
        }
    }

    public sealed class Compilation
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly ReadOnlyCollection<SchemaTableSymbol> _schemaTables;

        public Compilation(SyntaxTree syntaxTree, IList<SchemaTableSymbol> schemaTables)
        {
            _syntaxTree = syntaxTree;
            _schemaTables = new ReadOnlyCollection<SchemaTableSymbol>(schemaTables);
        }

        public SemanticModel GetSemanticModel()
        {
            var schemaBindingContext = new SchemaBindingConext(_schemaTables);
            var binder = new Binder(schemaBindingContext);
            var bindingResult = binder.Bind(_syntaxTree.Root);
            return new SemanticModel(this, bindingResult);
        }

        public Compilation SetSyntaxTree(SyntaxTree syntaxTree)
        {
            return _syntaxTree == syntaxTree ? this : new Compilation(syntaxTree, _schemaTables);
        }

        public Compilation SetSchemaTables(IList<SchemaTableSymbol> schemaTables)
        {
            return _schemaTables == schemaTables ? this : new Compilation(_syntaxTree, schemaTables);
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }

        public ReadOnlyCollection<SchemaTableSymbol> SchemaTables
        {
            get { return _schemaTables; }
        }
    }

    internal sealed class BindingResult
    {
        private readonly SyntaxNode _root;
        private readonly BoundNode _boundRoot;
        private readonly IDictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode;
        private readonly IDictionary<BoundNode, BindingContext> _bindingContextFromBoundNode;
        private readonly ReadOnlyCollection<Diagnostic> _diagnostics;

        public BindingResult(SyntaxNode root, BoundNode boundRoot, IDictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, IDictionary<BoundNode, BindingContext> bindingContextFromBoundNode, IList<Diagnostic> diagnostics)
        {
            _root = root;
            _boundRoot = boundRoot;
            _boundNodeFromSynatxNode = boundNodeFromSynatxNode;
            _bindingContextFromBoundNode = bindingContextFromBoundNode;
            _diagnostics = new ReadOnlyCollection<Diagnostic>(diagnostics);
        }

        public SyntaxNode Root
        {
            get { return _root; }
        }

        public BoundNode BoundRoot
        {
            get { return _boundRoot; }
        }

        public ReadOnlyCollection<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public BoundNode GetBoundNode(SyntaxNode syntaxNode)
        {
            BoundNode result;
            _boundNodeFromSynatxNode.TryGetValue(syntaxNode, out result);
            return result;
        }

        public BindingContext GetBindingContext(SyntaxNode syntaxNode)
        {
            var boundNode = GetBoundNode(syntaxNode);
            return boundNode == null ? null : GetBindingContext(boundNode);
        }

        public BindingContext GetBindingContext(BoundNode boundNode)
        {
            BindingContext result;
            _bindingContextFromBoundNode.TryGetValue(boundNode, out result);
            return result;
        }
    }

    internal sealed class Binder
    {
        private readonly SchemaBindingConext _schemaBindingConext;
        private readonly Stack<BindingContext> _bindingContextStack;
        private readonly Dictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode = new Dictionary<SyntaxNode, BoundNode>();
        private readonly Dictionary<BoundNode, BindingContext> _bindingContextFromBoundNode = new Dictionary<BoundNode, BindingContext>();
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public Binder(SchemaBindingConext schemaBindingConext)
        {
            _schemaBindingConext = schemaBindingConext;
            _bindingContextStack = new Stack<BindingContext>();
            _bindingContextStack.Push(schemaBindingConext);
        }

        private BindingContext Context
        {
            get { return _bindingContextStack.Peek(); }
        }

        public BindingResult Bind(CompilationUnitSyntax compilationUnit)
        {
            var boundRoot = BindRoot(compilationUnit.Root);
            return new BindingResult(compilationUnit.Root, boundRoot, _boundNodeFromSynatxNode, _bindingContextFromBoundNode, _diagnostics);
        }

        private BoundNode BindRoot(SyntaxNode root)
        {
            var query = root as QuerySyntax;
            if (query != null)
                return BindQuery(query);

            var expression = root as ExpressionSyntax;
            if (expression != null)
                return BindExpression(expression);

            throw new NotSupportedException();
        }

        private TResult Bind<TInput, TResult>(TInput node, Func<TInput, TResult> bindMethod)
            where TInput : SyntaxNode
            where TResult : BoundNode
        {
            var context = Context;
            var boundNode = bindMethod(node);
            
            _boundNodeFromSynatxNode.Add(node, boundNode);

            if (!_bindingContextFromBoundNode.ContainsKey(boundNode))
                _bindingContextFromBoundNode.Add(boundNode, context);

            return boundNode;
        }

        #region Expressions

        private BoundExpression BindExpression(ExpressionSyntax node)
        {
            return Bind(node, BindExpressionInternal);
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.ComplementExpression:
                case SyntaxKind.IdentityExpression:
                case SyntaxKind.NegationExpression:
                case SyntaxKind.LogicalNotExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax) node);

                case SyntaxKind.BitAndExpression:
                case SyntaxKind.BitOrExpression:
                case SyntaxKind.BitXorExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModulusExpression:
                case SyntaxKind.PowerExpression:
                case SyntaxKind.EqualExpression:
                case SyntaxKind.NotEqualExpression:
                case SyntaxKind.LessExpression:
                case SyntaxKind.LessOrEqualExpression:
                case SyntaxKind.GreaterExpression:
                case SyntaxKind.GreaterOrEqualExpression:
                case SyntaxKind.NotLessExpression:
                case SyntaxKind.NotGreaterExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.LogicalOrExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax) node);

                case SyntaxKind.LikeExpression:
                    return BindLikeExpression((LikeExpressionSyntax) node);

                case SyntaxKind.SoundslikeExpression:
                    return BindSoundslikeExpression((SoundslikeExpressionSyntax) node);

                case SyntaxKind.SimilarToExpression:
                    return BindSimilarToExpression((SimilarToExpressionSyntax) node);

                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax) node);

                case SyntaxKind.BetweenExpression:
                    return BindBetweenExpression((BetweenExpressionSyntax) node);

                case SyntaxKind.IsNullExpression:
                    return BindIsNullExpression((IsNullExpressionSyntax) node);

                case SyntaxKind.CastExpression:
                    return BindCastExpression((CastExpressionSyntax) node);

                case SyntaxKind.CaseExpression:
                    return BindCaseExpression((CaseExpressionSyntax) node);

                case SyntaxKind.CoalesceExpression:
                    return BindCoalesceExpression((CoalesceExpressionSyntax) node);

                case SyntaxKind.NullIfExpression:
                    return BindNullIfExpression((NullIfExpressionSyntax) node);

                case SyntaxKind.InExpression:
                    return BindInExpression((InExpressionSyntax) node);

                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax) node);

                case SyntaxKind.ParameterExpression:
                    return BindParameterExpression((ParameterExpressionSyntax) node);

                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax) node);

                case SyntaxKind.PropertyAccessExpression:
                    return BindPropertyAccessExpression((PropertyAccessExpressionSyntax) node);

                case SyntaxKind.CountAllExpression:
                    return BindCountAllExpression((CountAllExpressionSyntax) node);

                case SyntaxKind.FunctionInvocationExpression:
                    return BindFunctionInvocationExpression((FunctionInvocationExpressionSyntax) node);

                case SyntaxKind.MethodInvocationExpression:
                    return BindMethodInvocationExpression((MethodInvocationExpressionSyntax) node);

                case SyntaxKind.SingleRowSubselect:
                    return BindSingleRowSubselect((SingleRowSubselectSyntax) node);

                case SyntaxKind.ExistsSubselect:
                    return BindExistsSubselect((ExistsSubselectSyntax) node);

                case SyntaxKind.AllAnySubselect:
                    return BindAllAnySubselect((AllAnySubselectSyntax) node);

                default:
                    throw new ArgumentException(string.Format("Unknown node kind: {0}", node.Kind), "node");
            }
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax node)
        {
            var expression = BindExpression(node.Expression);
            // TODO: Resolve opeator
            return new BoundUnaryExpression(expression, null);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax node)
        {
            var left = BindExpression(node.Left);
            var right = BindExpression(node.Right);
            // TODO: Resolve operator
            return new BoundBinaryExpression(left, null, right);
        }

        private BoundExpression BindLikeExpression(LikeExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindSoundslikeExpression(SoundslikeExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindSimilarToExpression(SimilarToExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            return BindExpression(node.Expression);
        }

        private BoundExpression BindBetweenExpression(BetweenExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindIsNullExpression(IsNullExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindCastExpression(CastExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindCaseExpression(CaseExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindCoalesceExpression(CoalesceExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindNullIfExpression(NullIfExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindInExpression(InExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax node)
        {
            return new BoundLiteralExpression(node.Value);
        }

        private BoundExpression BindParameterExpression(ParameterExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax node)
        {
            var name = node.Identifier.Text;
            var symbols = Context.LookupSymbols(name, false).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.Add(DiagnosticFactory.UndeclaredEntity(node));
                var errorSymbol = new BadSymbol(name);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            if (symbols.Length > 1)
            {
                // TODO: Report ambiguous match                   
            }

            // TODO: Check that symbol resolves to a table instance, column, constant or parameter
            // TODO: If it's a function or aggregate report that parenthesis are required

            var first = symbols[0];
            return new BoundNameExpression(first, symbols);
        }

        private BoundExpression BindPropertyAccessExpression(PropertyAccessExpressionSyntax node)
        {
            // For cases like Foo.Bar we first check whether 'Foo' can be resolved to a
            // table instance, if that's the case we bind a column otherwise we bind a
            // normal expression.

            var targetAsName = node.Target as NameExpressionSyntax;
            if (targetAsName != null)
            {
                var symbols = Context.LookupSymbols(targetAsName.Identifier.Text, false).ToArray();
                if (symbols.Length == 1)
                {
                    var table = symbols[0] as TableInstanceSymbol;
                    if (table != null)
                    {
                        // TODO: This is a hack. The problem is that by skipping the name node
                        //       we end up never associating a bound node with the name expression node.
                        //
                        // I believe it would be best to introduce a Parent property and special case
                        // the table access in the name expression.

                        var boundNameNode = new BoundNameExpression(table, Enumerable.Empty<Symbol>());
                        _boundNodeFromSynatxNode.Add(targetAsName, boundNameNode);
                        _bindingContextFromBoundNode.Add(boundNameNode, Context);

                        return BindColumnInstance(node, table);
                    }
                }
            }

            var boundExpression = BindExpression(node.Target);
            var name = node.Name.Text;

            if (!boundExpression.Type.IsUnknown())
            {
                // TODO: Bind property
            }

            var errorSymbol = new BadSymbol(name);
            return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
        }

        private BoundExpression BindColumnInstance(PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var columnName = node.Name.Text;
            var columns = tableInstance.Table.Columns.Where(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (columns.Length == 0)
            {
                _diagnostics.Add(DiagnosticFactory.UndeclaredColumn(node, tableInstance));
                var errorSymbol = new BadSymbol(columnName);
                return new BoundNameExpression(errorSymbol, tableInstance.Table.Columns);
            }
            
            if (columns.Length > 1)
            {
                // TODO: Return ambiguous match
            }

            var column = columns.First();
            var columnInstance = new ColumnInstanceSymbol(tableInstance, column);
            return new BoundNameExpression(columnInstance, columns);
        }

        private BoundExpression BindCountAllExpression(CountAllExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindSingleRowSubselect(SingleRowSubselectSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindExistsSubselect(ExistsSubselectSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindAllAnySubselect(AllAnySubselectSyntax node)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Table References

        private BoundTableReference BindTableReference(TableReferenceSyntax node)
        {
            return Bind(node, BindTableReferenceInternal);
        }

        private BoundTableReference BindTableReferenceInternal(TableReferenceSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.ParenthesizedTableReference:
                    return BindParenthesizedTableReference((ParenthesizedTableReferenceSyntax) node);

                case SyntaxKind.NamedTableReference:
                    return BindNamedTableReference((NamedTableReferenceSyntax) node);

                case SyntaxKind.CrossJoinedTableReference:
                    return BindCrossJoinedTableReference((CrossJoinedTableReferenceSyntax) node);

                case SyntaxKind.InnerJoinedTableReference:
                    return BindInnerJoinedTableReference((InnerJoinedTableReferenceSyntax) node);

                case SyntaxKind.OuterJoinedTableReference:
                    return BindOuterJoinedTableReference((OuterJoinedTableReferenceSyntax) node);

                case SyntaxKind.DerivedTableReference:
                    return BindDerivedTableReference((DerivedTableReferenceSyntax) node);

                default:
                    throw new ArgumentException(string.Format("Unknown node kind: {0}", node.Kind), "node");
            }
        }

        private BoundTableReference BindParenthesizedTableReference(ParenthesizedTableReferenceSyntax node)
        {
            return BindTableReference(node.TableReference);
        }

        private BoundTableReference BindNamedTableReference(NamedTableReferenceSyntax node)
        {
            var symbols = Context.LookupSymbols(node.TableName.Text, false).OfType<TableSymbol>().ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.Add(DiagnosticFactory.UndeclaredTable(node));

                var badTableSymbol = new BadTableSymbol(node.TableName.Text);
                var badAlias = node.Alias == null
                                   ? badTableSymbol.Name
                                   : node.Alias.Identifier.Text;
                var errorInstance = new TableInstanceSymbol(badAlias, badTableSymbol);
                return new BoundNamedTableReference(errorInstance);
            }

            if (symbols.Length > 1)
            {
                // TODO: Report ambiguous match
            }

            var table = symbols[0];
            var alias = node.Alias == null
                            ? table.Name
                            : node.Alias.Identifier.Text;

            var tableInstance = new TableInstanceSymbol(alias, table);
            var boundNamedTableReference = new BoundNamedTableReference(tableInstance);

            var parent = _bindingContextStack.Pop();
            var bindingContext = new TableBindingContext(parent, boundNamedTableReference);
            _bindingContextStack.Push(bindingContext);

            return boundNamedTableReference;
        }

        private BoundTableReference BindCrossJoinedTableReference(CrossJoinedTableReferenceSyntax node)
        {
            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);

            var parent = _bindingContextStack.Pop();
            var bindingContext = new JoinConditionBindingContext(parent, left, right);
            _bindingContextStack.Push(bindingContext);

            return new BoundJoinedTableReference(BoundJoinType.InnerJoin, left, right, null);
        }

        private BoundTableReference BindInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
        {
            var parent = Context;

            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);
            _bindingContextStack.Pop();

            var bindingContext = new JoinConditionBindingContext(parent, left, right);
            _bindingContextStack.Push(bindingContext);

            var expressionBindingContext = new ExpressionBindingContext(bindingContext);
            _bindingContextStack.Push(expressionBindingContext);

            var condition = BindExpression(node.Condition);
            
            _bindingContextStack.Pop();

            // TODO: Ensure condition evaluates to boolean

            return new BoundJoinedTableReference(BoundJoinType.InnerJoin, left, right, condition);
        }

        private BoundTableReference BindOuterJoinedTableReference(OuterJoinedTableReferenceSyntax node)
        {
            var joinType = node.TypeKeyword.Kind == SyntaxKind.LeftKeyword
                               ? BoundJoinType.LeftOuterJoin
                               : node.TypeKeyword.Kind == SyntaxKind.RightKeyword
                                     ? BoundJoinType.RightOuterJoin
                                     : BoundJoinType.FullOuterJoin;

            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);

            var parent = _bindingContextStack.Pop();
            var bindingContext = new JoinConditionBindingContext(parent, left, right);
            _bindingContextStack.Push(bindingContext);

            var condition = BindExpression(node.Condition);

            // TODO: Ensure condition evaluates to boolean

            return new BoundJoinedTableReference(joinType, left, right, condition);
        }

        private BoundTableReference BindDerivedTableReference(DerivedTableReferenceSyntax node)
        {
            // TODO: We need to make sure the nested query doesn't get access to our variables.
            //
            // More precisely, the following query is expected to cause a resolution error
            // for "t.RegionId" in the WHERE clause. However, right now it succeeds and binds
            // agains the "outer row".
            //
            //    SELECT e.LastName + ', ' + e.FirstName Employee,
            //		     r.RegionDescription Regions,
            //		     t.TerritoryDescription Territories
            //
            //    FROM	 Employees e
            //			    INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            //			    INNER JOIN Territories t ON t.TerritoryID = et.TerritoryID
            //			    CROSS JOIN (SELECT * FROM Region x WHERE x.RegionID = t.RegionID) AS r
            //
            // One way to achieve this is by introducing a new "outermost" binding context. This is
            // either the SchemaBindingContext (in case of non-CTE tables) or the CteBindingContext
            // which contains the CTEs and has the SchemaBindingContext as its parent.
            //
            // When resolving a CTE, the current context is set to this outermost binding context.

            var previousBindingContext = _bindingContextStack.Pop();
            _bindingContextStack.Push(_schemaBindingConext);
            
            var query = BindQuery(node.Query);

            _bindingContextStack.Pop();
            _bindingContextStack.Push(previousBindingContext);

            var columns = (from c in query.SelectColumns
                           select new ColumnSymbol(c.Name, c.Expression.Type)).ToArray();

            var derivedTable = new DerivedTableSymbol(columns);
            var derivedTableInstance = new TableInstanceSymbol(node.Name.Text, derivedTable);
            var boundTableReference = new BoundDerivedTableReference(derivedTableInstance, query);

            var parent = _bindingContextStack.Pop();
            var bindingContext = new TableBindingContext(parent, boundTableReference);
            _bindingContextStack.Push(bindingContext);

            return boundTableReference;
        }

        #endregion
       
        #region Queries

        private BoundQuery BindQuery(QuerySyntax node)
        {
            return Bind(node, BindQueryInternal);
        }

        private BoundQuery BindQueryInternal(QuerySyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.ExceptQuery:
                    return BindExceptQuery((ExceptQuerySyntax)node);

                case SyntaxKind.UnionQuery:
                    return BindUnionQuery((UnionQuerySyntax)node);
                
                case SyntaxKind.IntersectQuery:
                    return BindIntersectQuery((IntersectQuerySyntax)node);
                
                case SyntaxKind.OrderedQuery:
                    return BindOrderedQuery((OrderedQuerySyntax)node);
                
                case SyntaxKind.ParenthesizedQuery:
                    return BindParenthesizedQuery((ParenthesizedQuerySyntax)node);
                
                case SyntaxKind.CommonTableExpressionQuery:
                    return BindCommonTableExpressionQuery((CommonTableExpressionQuerySyntax)node);
                
                case SyntaxKind.SelectQuery:
                    return BindSelectQuery((SelectQuerySyntax)node);

                default:
                    throw new ArgumentException(string.Format("Unknown node kind: {0}", node.Kind), "node");
            }
        }

        private BoundQuery BindExceptQuery(ExceptQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);

            return new BoundCombinedQuery(left, BoundQueryCombinator.Except, right);
        }

        private BoundQuery BindUnionQuery(UnionQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            var combinator = node.AllKeyword == null
                                 ? BoundQueryCombinator.Union
                                 : BoundQueryCombinator.UnionAll;

            return new BoundCombinedQuery(left, combinator, right);
        }

        private BoundQuery BindIntersectQuery(IntersectQuerySyntax node)
        {
            var left = BindQuery(node.LeftQuery);
            var right = BindQuery(node.RightQuery);
            return new BoundCombinedQuery(left, BoundQueryCombinator.Intersect, right);
        }

        private BoundQuery BindOrderedQuery(OrderedQuerySyntax node)
        {
            var query = BindQuery(node.Query);

            throw new NotImplementedException();
        }

        private BoundQuery BindParenthesizedQuery(ParenthesizedQuerySyntax node)
        {
            var query = BindQuery(node.Query);

            throw new NotImplementedException();
        }

        private BoundQuery BindCommonTableExpressionQuery(CommonTableExpressionQuerySyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundQuery BindSelectQuery(SelectQuerySyntax node)
        {
            var fromClause = BindFromClause(node.FromClause);
            var whereClause = BindWhereClause(node.WhereClause);
            var selectColumns = BindSelectColumns(node.SelectColumns);

            if (node.GroupByClause != null)
            {
                // TODO: Bind GroupByClause            
            }

            var havingClause = BindHavingClause(node.HavingClause);

            return new BoundSelectQuery(selectColumns, fromClause, whereClause, havingClause);
        }

        private IList<BoundSelectColumn> BindSelectColumns(IEnumerable<SelectColumnSyntax> nodes)
        {
            var result = new List<BoundSelectColumn>();
            foreach (var node in nodes)
            {
                switch (node.Kind)
                {
                    case SyntaxKind.ExpressionSelectColumn:
                        var boundColumn = BindExpressionSelectColumn((ExpressionSelectColumnSyntax) node);
                        result.Add(boundColumn);
                        break;

                    case SyntaxKind.WildcardSelectColumn:
                        var boundColumns = BindWildcardSelectColumn((WildcardSelectColumnSyntax) node);
                        result.AddRange(boundColumns);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Unknown column kind {0}.", node.Kind), "nodes");
                }
            }

            return result;
        }

        private BoundSelectColumn BindExpressionSelectColumn(ExpressionSelectColumnSyntax node)
        {
            var expression = BindExpression(node.Expression);
            var name = node.Alias != null
                           ? node.Alias.Identifier.Text
                           : InferColumnName(expression);
            return new BoundSelectColumn(expression, name);
        }

        private static string InferColumnName(BoundExpression expression)
        {
            var nameExpression = expression as BoundNameExpression;
            return nameExpression != null ? nameExpression.Symbol.Name : null;
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumn(WildcardSelectColumnSyntax node)
        {
            var tableName = node.TableName == null
                                ? null
                                : node.TableName.Value.Text;

            return tableName != null
                       ? BindWildcardSelectColumnForTable(tableName)
                       : BindWildcardSelectColumnForAllTables();
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumnForTable(string tableName)
        {
            var symbols = Context.LookupSymbols(tableName, false)
                                 .Where(s => s.Kind == SymbolKind.TableInstance)
                                 .Cast<TableInstanceSymbol>().ToArray();

            if (symbols.Length == 0)
            {
                // TODO: Report unresolved
                return Enumerable.Empty<BoundSelectColumn>();
            }

            if (symbols.Length > 1)
            {
                // TODO: Report ambiguous match
            }

            var tableInstance = symbols[0];
            return BindSelectColumnFromColumns(tableInstance);
        }

        private static IEnumerable<BoundSelectColumn> BindSelectColumnFromColumns(TableInstanceSymbol tableInstance)
        {
            return from column in tableInstance.Table.Columns
                   let expression = new BoundNameExpression(column, Enumerable.Empty<Symbol>())
                   select new BoundSelectColumn(expression, column.Name);
        }

        private IEnumerable<BoundSelectColumn> BindWildcardSelectColumnForAllTables()
        {
            var symbols = Context.LookupSymbols()
                                 .Where(s => s.Kind == SymbolKind.TableInstance)
                                 .Cast<TableInstanceSymbol>().ToArray();

            return from tableInstance in symbols
                   from column in BindSelectColumnFromColumns(tableInstance)
                   select column;
        }

        private BoundTableReference BindFromClause(FromClauseSyntax node)
        {
            if (node == null)
                return null;

            BindingContext lastBindingContext = null;
            BoundTableReference lastTableReference = null;

            var outerBindingContext = _bindingContextStack.Pop();

            foreach (var tableReference in node.TableReferences)
            {
                _bindingContextStack.Push(outerBindingContext);
                var boundTableReference = BindTableReference(tableReference);
                var tableReferenceBindingContext = _bindingContextStack.Pop();

                if (lastBindingContext == null)
                {
                    lastBindingContext = tableReferenceBindingContext;
                    lastTableReference = boundTableReference;
                }
                else
                {
                    lastBindingContext = new JoinConditionBindingContext(outerBindingContext, lastTableReference, boundTableReference);
                    lastTableReference = new BoundJoinedTableReference(BoundJoinType.InnerJoin, lastTableReference, boundTableReference, null);
                }
            }

            _bindingContextStack.Push(lastBindingContext);
            return lastTableReference;
        }

        private BoundExpression BindWhereClause(WhereClauseSyntax node)
        {
            if (node == null)
                return null;

            var boundWhereClause = BindExpression(node.Predicate);
            // TODO: Ensure where evaluates to boolean
            return boundWhereClause;
        }

        private BoundExpression BindHavingClause(HavingClauseSyntax node)
        {
            if (node == null)
                return null;

            var boundHavingClause = BindExpression(node.Predicate);
            // TODO: Ensure having evaluates to boolean

            return boundHavingClause;
        }

        #endregion
    }

    internal static class WellKnownTypes
    {
        private static class UnknownType { }
        private static class NullType { }
        private static class MissingType { }

        public static Type Unknown = typeof(UnknownType);
        public static Type Null = typeof(NullType);
        public static Type Missing = typeof(MissingType);

        public static bool IsMissing(this Type type)
        {
            return type == Missing;
        }

        public static bool IsNull(this Type type)
        {
            return type == Null;
        }

        public static bool IsUnknown(this Type type)
        {
            return type == Unknown;
        }
    }
}