using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NQuery.Language;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
        private readonly DataContext _dataContext;
        private readonly RootBindingContext _rootBindingContext;
        private readonly Stack<BindingContext> _bindingContextStack;
        private readonly Dictionary<SyntaxNode, BoundNode> _boundNodeFromSynatxNode = new Dictionary<SyntaxNode, BoundNode>();
        private readonly Dictionary<BoundNode, BindingContext> _bindingContextFromBoundNode = new Dictionary<BoundNode, BindingContext>();
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public Binder(DataContext dataContext)
        {
            _dataContext = dataContext;
            _rootBindingContext = new RootBindingContext(dataContext);
            _bindingContextStack = new Stack<BindingContext>();
            _bindingContextStack.Push(_rootBindingContext);
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
    }
}