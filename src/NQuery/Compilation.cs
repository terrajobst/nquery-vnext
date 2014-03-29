using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;
using NQuery.Optimization;
using NQuery.Symbols;

namespace NQuery
{
    public sealed class Compilation
    {
        private readonly DataContext _dataContext;
        private readonly SyntaxTree _syntaxTree;

        public Compilation(DataContext dataContext, SyntaxTree syntaxTree)
        {
            _dataContext = dataContext;
            _syntaxTree = syntaxTree;
        }

        public SemanticModel GetSemanticModel()
        {
            var bindingResult = Binder.Bind(_syntaxTree.Root, _dataContext);
            return new SemanticModel(this, bindingResult);
        }

        public CompiledQuery Compile()
        {
            var bindingResult = Binder.Bind(_syntaxTree.Root, _dataContext);
            var boundQuery = GetBoundQuery(bindingResult.BoundRoot);
            var diagnostics = GetDiagnostics(bindingResult);

            if (diagnostics.Any())
                throw new CompilationException(diagnostics);

            var optimizedQuery = Optimizer.Optimize(boundQuery);

            return new CompiledQuery(optimizedQuery);
        }

        private ImmutableArray<Diagnostic> GetDiagnostics(BindingResult bindingResult)
        {
            var syntaxDiagnostics = _syntaxTree.GetDiagnostics();
            var semanticDiagnostics = bindingResult.Diagnostics;
            return syntaxDiagnostics.Concat(semanticDiagnostics).ToImmutableArray();
        }

        public ShowPlan GetShowPlan()
        {
            return GetShowPlanSteps().LastOrDefault();
        }

        public IEnumerable<ShowPlan> GetShowPlanSteps()
        {
            var bindingResult = Binder.Bind(_syntaxTree.Root, _dataContext);

            if (GetDiagnostics(bindingResult).Any())
                yield break;

            var inputQuery = GetBoundQuery(bindingResult.BoundRoot);
            yield return ShowPlanBuilder.Build("Unoptimized", inputQuery);

            var relation = inputQuery.Relation;

            foreach (var rewriter in Optimizer.GetOptimizationSteps())
            {
                var step = rewriter.RewriteRelation(relation);
                if (step != relation)
                {
                    var stepName = string.Format("Optimization Step: {0}", rewriter.GetType().Name);
                    var stepQuery = new BoundQuery(step, inputQuery.OutputColumns);
                    yield return ShowPlanBuilder.Build(stepName, stepQuery);
                }

                relation = step;
            }

            var ouputQuery = new BoundQuery(relation, inputQuery.OutputColumns);
            yield return ShowPlanBuilder.Build("Optimized", ouputQuery);
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
            return _syntaxTree == syntaxTree ? this : new Compilation(_dataContext, syntaxTree);
        }

        public Compilation WithDataContext(DataContext dataContext)
        {
            return _dataContext == dataContext ? this : new Compilation(dataContext, _syntaxTree);
        }

        public static readonly Compilation Empty = new Compilation(DataContext.Empty, SyntaxTree.Empty);

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