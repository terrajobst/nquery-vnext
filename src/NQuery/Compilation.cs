using System;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery
{
    public sealed class Compilation
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly DataContext _dataContext;

        public Compilation(SyntaxTree syntaxTree, DataContext dataContext)
        {
            _syntaxTree = syntaxTree;
            _dataContext = dataContext;
        }

        public SemanticModel GetSemanticModel()
        {
            var bindingResult = Binder.Bind(_syntaxTree.Root, _dataContext);
            return new SemanticModel(this, bindingResult);
        }

        public CompiledResult Compile()
        {
            var bindingResult = Binder.Bind(_syntaxTree.Root, _dataContext);
            var boundQuery = GetBoundQuery(bindingResult.BoundRoot);

            var syntaxDiagnostics = _syntaxTree.GetDiagnostics();
            var semanticDiagnostics = bindingResult.Diagnostics;
            var diagnostics = syntaxDiagnostics.Concat(semanticDiagnostics).ToList().AsReadOnly();

            if (diagnostics.Any())
                throw new CompilationException(diagnostics);

            return new CompiledResult(boundQuery);
        }

        private static BoundQuery GetBoundQuery(BoundNode boundRoot)
        {
            if (boundRoot == null)
                return null;

            var query = boundRoot as BoundQuery;
            if (query != null)
                return query;

            var expression = (BoundExpression) boundRoot;
            return CreateBoundQuery(expression);
        }

        private static BoundQuery CreateBoundQuery(BoundExpression expression)
        {
            var valueSlot = new ValueSlot("result", expression.Type);
            var computedValue = new BoundComputedValue(expression, valueSlot);
            var constantRelation = new BoundConstantRelation();
            var computeRelation = new BoundComputeRelation(constantRelation, new[] {computedValue});
            var columnSymbol = new QueryColumnInstanceSymbol(valueSlot.Name, valueSlot);
            return new BoundQuery(computeRelation, new[] {columnSymbol});
        }

        public Compilation WithSyntaxTree(SyntaxTree syntaxTree)
        {
            return _syntaxTree == syntaxTree ? this : new Compilation(syntaxTree, _dataContext);
        }

        public Compilation WithDataContext(DataContext dataContext)
        {
            return _dataContext == dataContext ? this : new Compilation(_syntaxTree, dataContext);
        }

        public static readonly Compilation Empty = new Compilation(SyntaxTree.Empty, DataContext.Empty);

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }

        public DataContext DataContext
        {
            get { return _dataContext; }
        }
    }
}