using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed partial class Binder
    {
        private readonly DataContext _dataContext;
        private readonly BindingContext _bindingContext;
        private readonly Dictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode;
        private readonly Dictionary<BoundNode, BindingContext> _bindingContextFromBoundNode;
        private readonly List<Diagnostic> _diagnostics;

        private Binder(DataContext dataContext, BindingContext bindingContext, Dictionary<SyntaxNode, BoundNode> boundNodeFromSynatxNode, Dictionary<BoundNode, BindingContext> bindingContextFromBoundNode, List<Diagnostic> diagnostics)
        {
            _bindingContext = bindingContext;
            _boundNodeFromSynatxNode = boundNodeFromSynatxNode;
            _bindingContextFromBoundNode = bindingContextFromBoundNode;
            _diagnostics = diagnostics;
            _dataContext = dataContext;
        }

        private Binder GetBinder(BindingContext bindingContext)
        {
            return bindingContext == _bindingContext
                       ? this
                       : new Binder(_dataContext, bindingContext, _boundNodeFromSynatxNode, _bindingContextFromBoundNode, _diagnostics);
        }

        private Binder GetBinderWithAdditionalSymbols(IEnumerable<Symbol> symbols)
        {
            var bindingContext = new AdditionalSymbolsBindingContext(_bindingContext, symbols);
            return GetBinder(bindingContext);
        }

        private Binder GetBinderWithAdditionalSymbols(params Symbol[] symbols)
        {
            return GetBinderWithAdditionalSymbols(symbols.AsEnumerable());
        }

        public static BindingResult Bind(CompilationUnitSyntax compilationUnit, DataContext dataContext)
        {
            var bindingContext = new DataContextBindingContext(dataContext);
            var boundNodeFromSynatxNode = new Dictionary<SyntaxNode, BoundNode>();
            var bindingContextFromBoundNode = new Dictionary<BoundNode, BindingContext>();
            var diagnostics = new List<Diagnostic>();
            var binder = new Binder(dataContext, bindingContext, boundNodeFromSynatxNode, bindingContextFromBoundNode, diagnostics);

            var boundRoot = binder.BindRoot(compilationUnit.Root);
            return new BindingResult(compilationUnit, boundRoot, boundNodeFromSynatxNode, bindingContextFromBoundNode, diagnostics);
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

            if (!_bindingContextFromBoundNode.ContainsKey(boundNode))
                _bindingContextFromBoundNode.Add(boundNode, _bindingContext);

            return boundNode;
        }
    }
}