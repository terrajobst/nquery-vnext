using NQuery.Syntax;

namespace NQuery.Binding
{
    internal abstract partial class Binder
    {
        private readonly SharedBinderState _sharedBinderState;

        protected Binder(SharedBinderState sharedBinderState, Binder parent)
        {
            Parent = parent;
            _sharedBinderState = sharedBinderState;
        }

        public Binder Parent { get; }

        private List<Diagnostic> Diagnostics
        {
            get { return _sharedBinderState.Diagnostics; }
        }

        private ValueSlotFactory ValueSlotFactory
        {
            get { return _sharedBinderState.ValueSlotFactory; }
        }

        protected virtual bool InWhereClause
        {
            get { return Parent is not null && Parent.InWhereClause; }
        }

        protected virtual bool InOnClause
        {
            get { return Parent is not null && Parent.InOnClause; }
        }

        protected virtual bool InGroupByClause
        {
            get { return Parent is not null && Parent.InGroupByClause; }
        }

        protected virtual bool InAggregateArgument
        {
            get { return Parent is not null && Parent.InAggregateArgument; }
        }

        private Binder CreateLocalBinder(IEnumerable<Symbol> symbols)
        {
            return new LocalBinder(_sharedBinderState, this, symbols);
        }

        private Binder CreateLocalBinder(params Symbol[] symbols)
        {
            return CreateLocalBinder(symbols.AsEnumerable());
        }

        private Binder CreateJoinConditionBinder(BoundRelation left, BoundRelation right)
        {
            var leftTables = left.GetDeclaredTableInstances();
            var rightTables = right.GetDeclaredTableInstances();
            var tables = leftTables.Concat(rightTables);
            return new JoinConditionBinder(_sharedBinderState, this, tables);
        }

        private Binder CreateQueryBinder()
        {
            return new QueryBinder(_sharedBinderState, this);
        }

        private Binder CreateGroupByClauseBinder()
        {
            return new GroupByClauseBinder(_sharedBinderState, this);
        }

        private Binder CreateWhereClauseBinder()
        {
            return new WhereClauseBinder(_sharedBinderState, this);
        }

        private Binder CreateAggregateArgumentBinder()
        {
            return new AggregateArgumentBinder(_sharedBinderState, this);
        }

        public static BindingResult Bind(CompilationUnitSyntax compilationUnit, DataContext dataContext)
        {
            var sharedBinderState = new SharedBinderState();
            var binder = new GlobalBinder(sharedBinderState, dataContext);
            var boundRoot = binder.BindRoot(compilationUnit.Root);
            return new BindingResult(compilationUnit, boundRoot, sharedBinderState.BoundNodeFromSyntaxNode, sharedBinderState.BinderFromBoundNode, sharedBinderState.Diagnostics);
        }

        private BoundNode BindRoot(SyntaxNode root)
        {
            var query = root as QuerySyntax;
            if (query is not null)
                return BindQuery(query);

            var expression = root as ExpressionSyntax;
            if (expression is not null)
                return BindExpression(expression);

            throw ExceptionBuilder.UnexpectedValue(root);
        }

        private TResult Bind<TInput, TResult>(TInput node, Func<TInput, TResult> bindMethod)
            where TInput : SyntaxNode
            where TResult : BoundNode
        {
            var boundNode = bindMethod(node);

            Bind(node, boundNode);

            return boundNode;
        }

        private void Bind<TInput, TResult>(TInput node, TResult boundNode)
            where TInput : SyntaxNode
            where TResult : BoundNode
        {
            _sharedBinderState.BoundNodeFromSyntaxNode.Add(node, boundNode);
            if (!_sharedBinderState.BinderFromBoundNode.ContainsKey(boundNode))
                _sharedBinderState.BinderFromBoundNode.Add(boundNode, this);
        }

        private T GetBoundNode<T>(SyntaxNode node)
            where T : BoundNode
        {
            BoundNode result;
            _sharedBinderState.BoundNodeFromSyntaxNode.TryGetValue(node, out result);
            return result as T;
        }
    }
}