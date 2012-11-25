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

        protected Binder(Binder parent, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, Binder> binderFromBoundNode, List<Diagnostic> diagnostics)
        {
            _parent = parent;
            _boundNodeFromSynatxNode = boundNodeFromSynatxNode;
            _binderFromBoundNode = binderFromBoundNode;
            _diagnostics = diagnostics;
        }

        public Binder Parent
        {
            get { return _parent; }
        }

        private Binder CreateLocalBinder(IEnumerable<Symbol> symbols)
        {
            return new LocalBinder(this, _boundNodeFromSynatxNode, _binderFromBoundNode, _diagnostics, symbols);
        }

        private Binder CreateLocalBinder(params Symbol[] symbols)
        {
            return CreateLocalBinder(symbols.AsEnumerable());
        }

        public static BindingResult Bind(CompilationUnitSyntax compilationUnit, DataContext dataContext)
        {
            var boundNodeFromSynatxNode = new Dictionary<SyntaxNode, BoundNode>();
            var binderFromBoundNode = new Dictionary<BoundNode, Binder>();
            var diagnostics = new List<Diagnostic>();
            var binder = new GlobalBinder(boundNodeFromSynatxNode, binderFromBoundNode, diagnostics, dataContext);
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