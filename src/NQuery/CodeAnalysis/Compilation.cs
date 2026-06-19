using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Emit;
using NQuery.CodeAnalysis.Optimization;
using NQuery.CodeAnalysis.Planning;

namespace NQuery.CodeAnalysis;

public sealed class Compilation
{
    private Compilation(Catalog catalog, SyntaxTree syntaxTree)
    {
        Catalog = catalog;
        SyntaxTree = syntaxTree;
    }

    public static Compilation Create(Catalog catalog, SyntaxTree syntaxTree)
    {
        ThrowIfNull(catalog);
        ThrowIfNull(syntaxTree);

        return new Compilation(catalog, syntaxTree);
    }

    public SemanticModel GetSemanticModel()
    {
        var bindingResult = Binder.Bind(SyntaxTree.Root, Catalog);
        return new SemanticModel(this, bindingResult);
    }

    // The query pipeline: Bind -> Algebrize -> Optimize -> Plan -> Emit, mirroring the
    // sequence the differential tests drive.
    public CompiledQuery Compile()
    {
        var bindingResult = Binder.Bind(SyntaxTree.Root, Catalog);

        var diagnostics = GetDiagnostics(bindingResult);
        if (diagnostics.Any())
            throw new CompilationException(diagnostics);

        var logicalQuery = Algebrize(bindingResult);

        logicalQuery = LogicalOptimizer.Optimize(logicalQuery, Catalog);
        var physicalQuery = Planner.Plan(logicalQuery);

#if DEBUG
        // Assert the physical plan's slot references all resolve against the scope the
        // emitter will compile them in -- turning an otherwise cryptic slot-lookup failure
        // deep in emit into a clear, located error. Debug-only: omitted from release builds.
        PhysicalPlanVerifier.Verify(physicalQuery);
#endif

        var plan = Emitter.Emit(physicalQuery);

        return new CompiledQuery(plan);
    }

    // A top-level query binds to a BoundQuery; a bare expression (Expression<T>, e.g.
    // aggregate type resolution) binds to a BoundExpression. The algebrizer wraps the
    // latter in a one-row projection so both feed the same Optimize/Plan/Emit pipeline.
    private static LogicalQuery Algebrize(BindingResult bindingResult)
    {
        return bindingResult.BoundRoot switch
        {
            BoundQuery boundQuery => Algebrizer.Algebrize(boundQuery),
            BoundExpression boundExpression => Algebrizer.Algebrize(boundExpression),
            _ => throw ExceptionBuilder.UnexpectedValue(bindingResult.BoundRoot)
        };
    }

    private ImmutableArray<Diagnostic> GetDiagnostics(BindingResult bindingResult)
    {
        var syntaxDiagnostics = SyntaxTree.GetDiagnostics();
        var semanticDiagnostics = bindingResult.Diagnostics;
        return syntaxDiagnostics.Concat(semanticDiagnostics).ToImmutableArray();
    }

    public ShowPlan? GetShowPlan()
    {
        return GetShowPlanSteps().LastOrDefault();
    }

    // The show plan, mirroring Compile's stages: the algebrized (unoptimized) logical tree,
    // each logical optimization pass that changed it, the optimized logical tree, and finally
    // the physical plan the planner produced.
    public IEnumerable<ShowPlan> GetShowPlanSteps()
    {
        var bindingResult = Binder.Bind(SyntaxTree.Root, Catalog);

        if (GetDiagnostics(bindingResult).Any())
            yield break;

        var logicalQuery = Algebrize(bindingResult);

        yield return LogicalShowPlanBuilder.Build(Resources.ShowPlanUnoptimized, logicalQuery);

        var outputColumns = logicalQuery.OutputColumns;
        var root = logicalQuery.Root;

        foreach (var (name, stepRoot) in LogicalOptimizer.GetOptimizationSteps(root, Catalog))
        {
            var stepName = string.Format(Resources.ShowPlanStepFmt, name);
            yield return LogicalShowPlanBuilder.Build(stepName, new LogicalQuery(stepRoot, outputColumns));
            root = stepRoot;
        }

        var optimizedQuery = new LogicalQuery(root, outputColumns);
        yield return LogicalShowPlanBuilder.Build(Resources.ShowPlanOptimized, optimizedQuery);

        var physicalQuery = Planner.Plan(optimizedQuery);
        yield return PhysicalShowPlanBuilder.Build(Resources.ShowPlanPhysical, physicalQuery);
    }

    public Compilation WithSyntaxTree(SyntaxTree syntaxTree)
    {
        ThrowIfNull(syntaxTree);

        return SyntaxTree == syntaxTree ? this : Create(Catalog, syntaxTree);
    }

    public Compilation WithCatalog(Catalog catalog)
    {
        ThrowIfNull(catalog);

        return Catalog == catalog ? this : Create(catalog, SyntaxTree);
    }

    public static Compilation Empty { get; } = Create(Catalog.Empty, SyntaxTree.Empty);

    public SyntaxTree SyntaxTree { get; }

    public Catalog Catalog { get; }
}
