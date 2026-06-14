using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.Emit;
using NQuery.Optimization;
using NQuery.Planning;

namespace NQuery;

public sealed class Compilation
{
    private Compilation(DataContext dataContext, SyntaxTree syntaxTree)
    {
        DataContext = dataContext;
        SyntaxTree = syntaxTree;
    }

    public static Compilation Create(DataContext dataContext, SyntaxTree syntaxTree)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(syntaxTree);

        return new Compilation(dataContext, syntaxTree);
    }

    public SemanticModel GetSemanticModel()
    {
        var bindingResult = Binder.Bind(SyntaxTree.Root, DataContext);
        return new SemanticModel(this, bindingResult);
    }

    // The query pipeline: Bind -> Algebrize -> Optimize -> Plan -> Emit, mirroring the
    // sequence the differential tests drive.
    public CompiledQuery Compile()
    {
        var bindingResult = Binder.Bind(SyntaxTree.Root, DataContext);

        var diagnostics = GetDiagnostics(bindingResult);
        if (diagnostics.Any())
            throw new CompilationException(diagnostics);

        var logicalQuery = Algebrize(bindingResult);

        logicalQuery = LogicalOptimizer.Optimize(logicalQuery, DataContext);
        var physicalQuery = Planner.Plan(logicalQuery);
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

    public ShowPlan GetShowPlan()
    {
        return GetShowPlanSteps().LastOrDefault();
    }

    // The show plan, mirroring Compile's stages: the algebrized (unoptimized) logical tree,
    // each logical optimization pass that changed it, the optimized logical tree, and finally
    // the physical plan the planner produced.
    public IEnumerable<ShowPlan> GetShowPlanSteps()
    {
        var bindingResult = Binder.Bind(SyntaxTree.Root, DataContext);

        if (GetDiagnostics(bindingResult).Any())
            yield break;

        var logicalQuery = Algebrize(bindingResult);

        yield return LogicalShowPlanBuilder.Build(Resources.ShowPlanUnoptimized, logicalQuery);

        var outputColumns = logicalQuery.OutputColumns;
        var root = logicalQuery.Root;

        foreach (var (name, stepRoot) in LogicalOptimizer.GetOptimizationSteps(root, DataContext))
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

        return SyntaxTree == syntaxTree ? this : Create(DataContext, syntaxTree);
    }

    public Compilation WithDataContext(DataContext dataContext)
    {
        ThrowIfNull(dataContext);

        return DataContext == dataContext ? this : Create(dataContext, SyntaxTree);
    }

    public static readonly Compilation Empty = Create(DataContext.Empty, SyntaxTree.Empty);

    public SyntaxTree SyntaxTree { get; }

    public DataContext DataContext { get; }
}
