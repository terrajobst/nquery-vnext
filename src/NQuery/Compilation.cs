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
        private Compilation(DataContext dataContext, SyntaxTree syntaxTree)
        {
            DataContext = dataContext;
            SyntaxTree = syntaxTree;
        }

        public static Compilation Create(DataContext dataContext, SyntaxTree syntaxTree)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            if (syntaxTree == null)
                throw new ArgumentNullException(nameof(syntaxTree));

            return new Compilation(dataContext, syntaxTree);
        }

        public SemanticModel GetSemanticModel()
        {
            var bindingResult = Binder.Bind(SyntaxTree.Root, DataContext);
            return new SemanticModel(this, bindingResult);
        }

        public CompiledQuery Compile()
        {
            var bindingResult = Binder.Bind(SyntaxTree.Root, DataContext);
            var boundQuery = GetBoundQuery(bindingResult.BoundRoot);
            var diagnostics = GetDiagnostics(bindingResult);

            if (diagnostics.Any())
                throw new CompilationException(diagnostics);

            var optimizedQuery = Optimizer.Optimize(boundQuery);

            return new CompiledQuery(optimizedQuery);
        }

        private ImmutableArray<Diagnostic> GetDiagnostics(BindingResult bindingResult)
        {
            var syntaxDiagnostics = SyntaxTree.GetDiagnostics();
            var semanticDiagnostics = bindingResult.Diagnostics;
            return syntaxDiagnostics.Concat(semanticDiagnostics).ToImmutableArray();
        }

        public ShowPlan GetShowPlan()
        {
            return GetShowPlanSteps().LastOrDefault();
        }

        public IEnumerable<ShowPlan> GetShowPlanSteps()
        {
            var bindingResult = Binder.Bind(SyntaxTree.Root, DataContext);

            if (GetDiagnostics(bindingResult).Any())
                yield break;

            var inputQuery = GetBoundQuery(bindingResult.BoundRoot);
            yield return ShowPlanBuilder.Build(Resources.ShowPlanUnoptimized, inputQuery);

            var relation = inputQuery.Relation;

            foreach (var rewriter in Optimizer.GetOptimizationSteps())
            {
                var step = rewriter.RewriteRelation(relation);
                if (step != relation)
                {
                    var stepName = string.Format(Resources.ShowPlanStepFmt, rewriter.GetType().Name);
                    var stepQuery = new BoundQuery(step, inputQuery.OutputColumns);
                    yield return ShowPlanBuilder.Build(stepName, stepQuery);
                }

                relation = step;
            }

            var ouputQuery = new BoundQuery(relation, inputQuery.OutputColumns);
            yield return ShowPlanBuilder.Build(Resources.ShowPlanOptimized, ouputQuery);
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
            var factory = new ValueSlotFactory();
            var valueSlot = new ValueSlot(factory, @"result", 0, expression.Type);
            var computedValue = new BoundComputedValue(expression, valueSlot);
            var constantRelation = new BoundConstantRelation();
            var computeRelation = new BoundComputeRelation(constantRelation, new[] {computedValue});
            var projectRelation = new BoundProjectRelation(computeRelation, new [] { valueSlot });
            var columnSymbol = new QueryColumnInstanceSymbol(valueSlot.Name, valueSlot);
            return new BoundQuery(projectRelation, new[] {columnSymbol});
        }

        public Compilation WithSyntaxTree(SyntaxTree syntaxTree)
        {
            if (syntaxTree == null)
                throw new ArgumentNullException(nameof(syntaxTree));

            return SyntaxTree == syntaxTree ? this : Create(DataContext, syntaxTree);
        }

        public Compilation WithDataContext(DataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            return DataContext == dataContext ? this : Create(dataContext, SyntaxTree);
        }

        public static readonly Compilation Empty = Create(DataContext.Empty, SyntaxTree.Empty);

        public SyntaxTree SyntaxTree { get; }

        public DataContext DataContext { get; }
    }
}