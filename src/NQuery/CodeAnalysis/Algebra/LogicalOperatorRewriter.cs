using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

// Bottom-up rewriter over the logical operator tree. The default for each node
// rewrites its child operators and rebuilds only if something changed (identity
// short-circuit), carrying expressions and slots through unchanged. Passes
// override the handful of nodes they care about and call base to get the
// children rewritten first.
internal abstract class LogicalOperatorRewriter
{
    public virtual LogicalOperator RewriteRelation(LogicalOperator node)
    {
        switch (node.Kind)
        {
            case LogicalOperatorKind.Empty:
                return RewriteEmpty((LogicalEmpty)node);
            case LogicalOperatorKind.Constant:
                return RewriteConstant((LogicalConstant)node);
            case LogicalOperatorKind.TableScan:
                return RewriteTableScan((LogicalTableScan)node);
            case LogicalOperatorKind.Filter:
                return RewriteFilter((LogicalFilter)node);
            case LogicalOperatorKind.Compute:
                return RewriteCompute((LogicalCompute)node);
            case LogicalOperatorKind.Project:
                return RewriteProject((LogicalProject)node);
            case LogicalOperatorKind.Join:
                return RewriteJoin((LogicalJoin)node);
            case LogicalOperatorKind.Apply:
                return RewriteApply((LogicalApply)node);
            case LogicalOperatorKind.Aggregate:
                return RewriteAggregate((LogicalAggregate)node);
            case LogicalOperatorKind.Union:
                return RewriteUnion((LogicalUnion)node);
            case LogicalOperatorKind.RecursiveUnion:
                return RewriteRecursiveUnion((LogicalRecursiveUnion)node);
            case LogicalOperatorKind.RecursiveReference:
                return RewriteRecursiveReference((LogicalRecursiveReference)node);
            case LogicalOperatorKind.IntersectOrExcept:
                return RewriteIntersectOrExcept((LogicalIntersectOrExcept)node);
            case LogicalOperatorKind.Sort:
                return RewriteSort((LogicalSort)node);
            case LogicalOperatorKind.Top:
                return RewriteTop((LogicalTop)node);
            case LogicalOperatorKind.Assert:
                return RewriteAssert((LogicalAssert)node);
            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }

    protected virtual LogicalOperator RewriteEmpty(LogicalEmpty node) => node;

    protected virtual LogicalOperator RewriteConstant(LogicalConstant node) => node;

    protected virtual LogicalOperator RewriteTableScan(LogicalTableScan node) => node;

    protected virtual LogicalOperator RewriteFilter(LogicalFilter node)
    {
        var input = RewriteRelation(node.Input);
        return input == node.Input ? node : new LogicalFilter(input, node.Conditions);
    }

    protected virtual LogicalOperator RewriteCompute(LogicalCompute node)
    {
        var input = RewriteRelation(node.Input);
        return input == node.Input ? node : new LogicalCompute(input, node.DefinedValues);
    }

    protected virtual LogicalOperator RewriteProject(LogicalProject node)
    {
        var input = RewriteRelation(node.Input);
        return input == node.Input ? node : new LogicalProject(input, node.Outputs);
    }

    protected virtual LogicalOperator RewriteJoin(LogicalJoin node)
    {
        var left = RewriteRelation(node.Left);
        var right = RewriteRelation(node.Right);
        return left == node.Left && right == node.Right
            ? node
            : new LogicalJoin(node.JoinKind, left, right, node.Conditions, node.Probe, node.PassthruPredicate);
    }

    protected virtual LogicalOperator RewriteApply(LogicalApply node)
    {
        var left = RewriteRelation(node.Left);
        var right = RewriteRelation(node.Right);
        return left == node.Left && right == node.Right
            ? node
            : new LogicalApply(node.ApplyKind, left, right, node.Probe, node.Passthru);
    }

    protected virtual LogicalOperator RewriteAggregate(LogicalAggregate node)
    {
        var input = RewriteRelation(node.Input);
        return input == node.Input ? node : new LogicalAggregate(input, node.Groups, node.Aggregates);
    }

    protected virtual LogicalOperator RewriteUnion(LogicalUnion node)
    {
        var inputs = RewriteMany(node.Inputs);
        return inputs == node.Inputs ? node : new LogicalUnion(node.IsUnionAll, inputs, node.DefinedValues, node.Comparers);
    }

    // The recursion boundary is opaque: passes rewrite the anchor and member subtrees
    // normally but never move anything across the union or touch its unified outputs.
    protected virtual LogicalOperator RewriteRecursiveUnion(LogicalRecursiveUnion node)
    {
        var anchor = RewriteRelation(node.Anchor);
        var members = RewriteMany(node.RecursiveMembers);
        return anchor == node.Anchor && members == node.RecursiveMembers
            ? node
            : new LogicalRecursiveUnion(node.Token, anchor, members, node.DefinedValues);
    }

    protected virtual LogicalOperator RewriteRecursiveReference(LogicalRecursiveReference node) => node;

    protected virtual LogicalOperator RewriteIntersectOrExcept(LogicalIntersectOrExcept node)
    {
        var left = RewriteRelation(node.Left);
        var right = RewriteRelation(node.Right);
        return left == node.Left && right == node.Right
            ? node
            : new LogicalIntersectOrExcept(node.IsIntersect, left, right, node.Comparers);
    }

    protected virtual LogicalOperator RewriteSort(LogicalSort node)
    {
        var input = RewriteRelation(node.Input);
        return input == node.Input ? node : new LogicalSort(node.IsDistinct, input, node.SortedValues);
    }

    protected virtual LogicalOperator RewriteTop(LogicalTop node)
    {
        var input = RewriteRelation(node.Input);
        return input == node.Input ? node : new LogicalTop(input, node.Limit, node.TieEntries);
    }

    protected virtual LogicalOperator RewriteAssert(LogicalAssert node)
    {
        var input = RewriteRelation(node.Input);
        return input == node.Input ? node : new LogicalAssert(input, node.Condition, node.Message);
    }

    private ImmutableArray<LogicalOperator> RewriteMany(ImmutableArray<LogicalOperator> inputs)
    {
        ImmutableArray<LogicalOperator>.Builder? builder = null;

        for (var i = 0; i < inputs.Length; i++)
        {
            var rewritten = RewriteRelation(inputs[i]);
            if (rewritten != inputs[i] && builder is null)
                builder = inputs.ToBuilder();
            if (builder is not null)
                builder[i] = rewritten;
        }

        return builder?.MoveToImmutable() ?? inputs;
    }
}
