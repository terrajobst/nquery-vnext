using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class ValueSlot
    {
        private readonly string _name;
        private readonly Type _type;

        public ValueSlot(string name, Type type)
        {
            _name = name;
            _type = type;
        }

        public string Name
        {
            get { return _name; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return _name;
        }
    }

    internal sealed class ValueSlotFactory
    {
        private readonly Dictionary<string, int> _usedNames = new Dictionary<string, int>();
        private int _nextTemporaryNumber = 1000;

        public ValueSlot CreateTemporaryValueSlot(Type type)
        {
            var name = string.Format("Expr{0}", _nextTemporaryNumber);
            _nextTemporaryNumber++;
            return new ValueSlot(name, type);
        }

        public ValueSlot CreateValueSlot(string name, Type type)
        {
            int highestNumber;
            _usedNames.TryGetValue(name, out highestNumber);

            highestNumber++;
            _usedNames[name] = highestNumber;

            var qualifiedName = name + ":" + highestNumber;
            return new ValueSlot(qualifiedName, type);
        }
    }

    internal sealed class ExpressionValueSlotFactory
    {
        private readonly ValueSlotFactory _valueSlotFactory;
        private readonly List<Tuple<ExpressionSyntax, ValueSlot, BoundExpression>> _localValues = new List<Tuple<ExpressionSyntax, ValueSlot, BoundExpression>>();

        public ExpressionValueSlotFactory(ValueSlotFactory valueSlotFactory)
        {
            _valueSlotFactory = valueSlotFactory;
        }

        public ValueSlot GetValueSlot(ExpressionSyntax expression)
        {
            return _localValues
                   .Where(t => t.Item1.IsEquivalentTo(expression))
                   .Select(t => t.Item2)
                   .FirstOrDefault();
        }

        public ValueSlot GetOrCreateValueSlot(ExpressionSyntax expression, BoundExpression boundExpression)
        {
            var existing = GetValueSlot(expression);

            if (existing != null)
                return existing;

            var boundNameExpression = boundExpression as BoundNameExpression;
            var columnInstanceSymbol = boundNameExpression == null
                                           ? null
                                           : boundNameExpression.Symbol as ColumnInstanceSymbol;

            if (columnInstanceSymbol != null)
                return columnInstanceSymbol.ValueSlot;

            var valueSlot = _valueSlotFactory.CreateTemporaryValueSlot(boundExpression.Type);
            _localValues.Add(Tuple.Create(expression, valueSlot, boundExpression));
            return valueSlot;
        }

        public IEnumerable<Tuple<ValueSlot, BoundExpression>> LocalValues
        {
            get { return _localValues.Select(t => Tuple.Create(t.Item2, t.Item3)); }
        }
    }

    internal sealed class QueryBinder : LocalBinder
    {
        // Access to all table & column instances
        // Should create unique value slots per aggregate/argument combination

        private readonly ExpressionValueSlotFactory _expressionValueSlotFactory;

        public QueryBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory, IEnumerable<Symbol> localSymbols)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory, localSymbols)
        {
            _expressionValueSlotFactory = new ExpressionValueSlotFactory(valueSlotFactory);
        }

        protected override bool InWhereClause
        {
            get { return false; }
        }

        protected override bool InOnClause
        {
            get { return false; }
        }

        protected override bool InGroupByClause
        {
            get { return false; }
        }

        protected override bool InAggregateArgument
        {
            get { return false; }
        }

        protected override ExpressionValueSlotFactory ExpressionValueSlotFactory
        {
            get { return _expressionValueSlotFactory; }
        }

        protected override BoundExpression BindAggregate(ExpressionSyntax aggregate, BoundAggregateExpression boundAggregate)
        {
            ExpressionValueSlotFactory.GetOrCreateValueSlot(aggregate, boundAggregate);
            return boundAggregate;
        }
    }

    internal sealed class AggregateArgumentBinder : Binder
    {
        public AggregateArgumentBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
        {
        }

        protected override bool InAggregateArgument
        {
            get { return true; }
        }
    }

    internal sealed class JoinConditionBinder : LocalBinder
    {
        public JoinConditionBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory, IEnumerable<Symbol> localSymbols)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory, localSymbols)
        {
        }

        protected override bool InOnClause
        {
            get { return true; }
        }
    }

    internal sealed class WhereClauseBinder : Binder
    {
        public WhereClauseBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
        {
        }

        protected override bool InWhereClause
        {
            get { return true; }
        }
    }

    internal sealed class GroupByClauseBinder : Binder
    {
        public GroupByClauseBinder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics, ValueSlotFactory valueSlotFactory)
            : base(parent, boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, valueSlotFactory)
        {
        }

        protected override bool InGroupByClause
        {
            get { return true; }
        }
    }

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

        private Binder CreateQueryBinder(BoundTableReference fromClause)
        {
            var symbols = fromClause == null
                              ? Enumerable.Empty<Symbol>()
                              : fromClause.GetDeclaredTableInstances();
            return new QueryBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, _valueSlotFactory, symbols);
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

            _boundNodeFromSynatxNode.Add(node, boundNode);

            if (!_binderFromBoundNode.ContainsKey(boundNode))
                _binderFromBoundNode.Add(boundNode, this);

            return boundNode;
        }
    }
}