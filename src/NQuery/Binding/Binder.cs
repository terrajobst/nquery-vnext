using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal abstract partial class Binder
    {
        private readonly Binder _parent;
        private readonly Dictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode;
        private readonly Dictionary<BoundNode, Binder> _binderFromBoundNode;
        private readonly List<Diagnostic> _diagnostics;
        private readonly ValueSlotFactory _valueSlotFactory;

        protected Binder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory)
        {
            _parent = parent;
            _boundNodeFromSynatxNode = boundNodeFromSynatxNode;
            _binderFromBoundNode = binderFromBoundNode;
            _diagnostics = diagnostics;
            _valueSlotFactory = valueSlotFactory;
        }

        public Binder Parent
        {
            get { return _parent; }
        }

        protected List<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        protected virtual bool InWhereClause
        {
            get { return _parent != null && _parent.InWhereClause; }
        }

        protected virtual bool InOnClause
        {
            get { return _parent != null && _parent.InOnClause; }
        }

        protected virtual bool InGroupByClause
        {
            get { return _parent != null && _parent.InGroupByClause; }
        }

        protected virtual bool InAggregateArgument
        {
            get { return _parent != null && _parent.InAggregateArgument; }
        }

        private Binder CreateLocalBinder(IEnumerable<Symbol> symbols)
        {
            return new LocalBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, _valueSlotFactory, symbols);
        }

        private Binder CreateLocalBinder(params Symbol[] symbols)
        {
            return CreateLocalBinder(symbols.AsEnumerable());
        }

        private Binder CreateJoinConditionBinder(BoundTableReference left, BoundTableReference right)
        {
            var leftTables = left.GetDeclaredTableInstances();
            var rightTables = right.GetDeclaredTableInstances();
            var tables = leftTables.Concat(rightTables);
            return new JoinConditionBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, _valueSlotFactory, tables);
        }

        private Binder CreateQueryBinder()
        {
            return new QueryBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, _valueSlotFactory);
        }

        private Binder CreateGroupByClauseBinder()
        {
            return new GroupByClauseBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, _valueSlotFactory);
        }

        private Binder CreateWhereClauseBinder()
        {
            return new WhereClauseBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, _valueSlotFactory);
        }

        private Binder CreateAggregateArgumentBinder()
        {
            return new AggregateArgumentBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, _valueSlotFactory);
        }

        public static BindingResult Bind(CompilationUnitSyntax compilationUnit, DataContext dataContext)
        {
            var boundNodeFromSynatxNode = new Dictionary<SyntaxNode, BoundNode>();
            var binderFromBoundNode = new Dictionary<BoundNode, Binder>();
            var diagnostics = new List<Diagnostic>();
            var valueSlotFactory = new ValueSlotFactory();
            var binder = new GlobalBinder(boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory, dataContext);
            var boundRoot = binder.BindRoot(compilationUnit.Root);
            return new BindingResult(compilationUnit, boundRoot, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics);
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
            var boundNode = bindMethod(node);

            Bind(node, boundNode);

            return boundNode;
        }

        private void Bind<TInput, TResult>(TInput node, TResult boundNode)
            where TInput : SyntaxNode
            where TResult : BoundNode
        {
            _boundNodeFromSynatxNode.Add(node, boundNode);
            if (!_binderFromBoundNode.ContainsKey(boundNode))
                _binderFromBoundNode.Add(boundNode, this);
        }

        private T GetBoundNode<T>(SyntaxNode node)
            where T : BoundNode
        {
            BoundNode result;
            _boundNodeFromSynatxNode.TryGetValue(node, out result);
            return result as T;
        }
    }
}