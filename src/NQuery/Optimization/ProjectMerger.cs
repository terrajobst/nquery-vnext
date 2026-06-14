#nullable enable

using NQuery.Algebra;

namespace NQuery.Optimization;

// Collapses redundant projections that the algebraic rewrites leave behind. Two
// shapes, both handled bottom-up so a stack of any height settles in one pass:
//
//   * adjacent projects -- Project_y(Project_x(g)) -> Project_y(g), keeping the outer
//     list y and discarding the inner list x outright (they need not match). This is
//     sound for any x because a project exposes its input's *defined* set, not just
//     its outputs (LogicalProject.ComputeDefinedValueSlots), so y is in scope on the
//     grandchild g even when y reads a slot x had projected away. Dropping the middle
//     layer changes neither the defined set nor the output set.
//   * identity project -- a project whose outputs equal its input's outputs, in the
//     same order, neither drops, adds, nor reorders anything, so it is a no-op and
//     the input stands in for it.
//
// Decorrelation is the main source: ApplyPushdown's project push-through wraps a
// projection around DecorrelateAggregate's own trailing projection, producing two
// identical layers. ColumnPruner only narrows a project's columns; it never removes
// the operator, so without this pass the layers survive into the physical plan.
internal sealed class ProjectMerger : LogicalOperatorRewriter
{
    public static readonly ProjectMerger Instance = new();

    private ProjectMerger()
    {
    }

    protected override LogicalOperator RewriteProject(LogicalProject node)
    {
        // Rewrite the input first, so any projects below are already collapsed; the
        // child is then at most a single project over a non-project.
        var rewritten = (LogicalProject)base.RewriteProject(node);

        // Merge a project directly over a project: project the outer's outputs off the
        // inner's input and drop the inner layer.
        while (rewritten.Input is LogicalProject inner)
            rewritten = new LogicalProject(inner.Input, rewritten.Outputs);

        // An identity project (same outputs, same order) adds nothing -- remove it.
        if (rewritten.Outputs.SequenceEqual(rewritten.Input.OutputValueSlots))
            return rewritten.Input;

        return rewritten;
    }
}
