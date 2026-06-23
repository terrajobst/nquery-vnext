using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Optimization;

namespace NQuery.Tests.CodeAnalysis.Optimization;

// The logical-plan verifier runs (in DEBUG) after every optimizer pass that changes
// the tree, so the whole suite exercises its happy path. These tests pin the failure
// path: a malformed tree must be rejected with a located, actionable error, and the
// apply-vs-join scoping distinction (an apply exposes its left to its right; a join
// does not) must be enforced.
public class LogicalPlanVerifierTests
{
    private const string Source = "test";

    [Fact]
    public void LogicalPlanVerifier_Accepts_WellFormedPlan()
    {
        var factory = new ValueSlotFactory();
        var slot = factory.CreateNamed("c", typeof(bool));
        var filter = new LogicalFilter(Leaf(slot), ImmutableArray.Create<LogicalExpression>(new LogicalValueSlotExpression(slot)));

        LogicalPlanVerifier.Verify(filter, Source);
    }

    [Fact]
    public void LogicalPlanVerifier_Rejects_FilterReferencingUnavailableSlot()
    {
        var factory = new ValueSlotFactory();
        var ghost = factory.CreateNamed("ghost", typeof(bool));
        var filter = new LogicalFilter(new LogicalConstant(), ImmutableArray.Create<LogicalExpression>(new LogicalValueSlotExpression(ghost)));

        var exception = Assert.Throws<InvalidOperationException>(() => LogicalPlanVerifier.Verify(filter, Source));
        Assert.Contains(ghost.Name, exception.Message);
    }

    [Fact]
    public void LogicalPlanVerifier_Accepts_ApplyRightReferencingLeft()
    {
        // The legitimate correlation: the apply's right references a left slot.
        var factory = new ValueSlotFactory();
        var leftSlot = factory.CreateNamed("l", typeof(bool));
        var rightSlot = factory.CreateNamed("r", typeof(bool));

        var right = new LogicalFilter(Leaf(rightSlot), ImmutableArray.Create<LogicalExpression>(new LogicalValueSlotExpression(leftSlot)));
        var apply = new LogicalApply(LogicalApplyKind.Inner, Leaf(leftSlot), right, probe: null);

        LogicalPlanVerifier.Verify(apply, Source);
    }

    [Fact]
    public void LogicalPlanVerifier_Rejects_JoinRightReferencingLeft()
    {
        // A logical join's sides are independent: its right may not reference the left
        // (that would have to be modeled as an apply). The reference is out of scope.
        var factory = new ValueSlotFactory();
        var leftSlot = factory.CreateNamed("l", typeof(bool));
        var rightSlot = factory.CreateNamed("r", typeof(bool));

        var right = new LogicalFilter(Leaf(rightSlot), ImmutableArray.Create<LogicalExpression>(new LogicalValueSlotExpression(leftSlot)));
        var join = new LogicalJoin(LogicalJoinKind.Inner, Leaf(leftSlot), right, ImmutableArray<LogicalExpression>.Empty, probe: null, passthruPredicate: null);

        var exception = Assert.Throws<InvalidOperationException>(() => LogicalPlanVerifier.Verify(join, Source));
        Assert.Contains(leftSlot.Name, exception.Message);
    }

    [Fact]
    public void LogicalPlanVerifier_Rejects_SlotIntroducedTwice()
    {
        // A malformed tree in which the same slot is minted on both sides of a join --
        // the shape a rewrite that duplicated a subtree without re-minting its slots
        // would produce. Each side is well-scoped on its own, so only the uniqueness
        // check catches it.
        var factory = new ValueSlotFactory();
        var slot = factory.CreateNamed("dup", typeof(bool));
        var join = new LogicalJoin(LogicalJoinKind.Inner, Leaf(slot), Leaf(slot), ImmutableArray<LogicalExpression>.Empty, probe: null, passthruPredicate: null);

        var exception = Assert.Throws<InvalidOperationException>(() => LogicalPlanVerifier.Verify(join, Source));
        Assert.Contains(slot.Name, exception.Message);
    }

    [Fact]
    public void LogicalPlanVerifier_Accepts_SlotThreadedThroughManyOperators()
    {
        // A slot defined once at the leaf and merely passed up through Filter/Project is
        // not a double definition: only the leaf introduces it.
        var factory = new ValueSlotFactory();
        var slot = factory.CreateNamed("c", typeof(bool));
        var filter = new LogicalFilter(Leaf(slot), ImmutableArray.Create<LogicalExpression>(new LogicalValueSlotExpression(slot)));
        var project = new LogicalProject(filter, ImmutableArray.Create(slot));

        LogicalPlanVerifier.Verify(project, Source);
    }

    [Fact]
    public void LogicalPlanVerifier_Failure_NamesSourceOperatorAndScope()
    {
        // The message must be actionable: it names the source (here, the pass), the
        // operator and role, the offending slot, and what is in scope.
        var factory = new ValueSlotFactory();
        var ghost = factory.CreateNamed("ghost", typeof(bool));
        var present = factory.CreateNamed("present", typeof(bool));
        var input = Leaf(present);
        var filter = new LogicalFilter(input, ImmutableArray.Create<LogicalExpression>(new LogicalValueSlotExpression(ghost)));

        var exception = Assert.Throws<InvalidOperationException>(() => LogicalPlanVerifier.Verify(filter, "after the 'SomePass' pass"));

        Assert.Contains("SomePass", exception.Message);
        Assert.Contains("Filter", exception.Message);
        Assert.Contains(ghost.Name, exception.Message);
        Assert.Contains(present.Name, exception.Message); // the in-scope listing
    }

    // A minimal leaf that outputs exactly the given slots: a compute of a literal per
    // slot over a one-row constant. The verifier only inspects slot identity.
    private static LogicalOperator Leaf(params ValueSlot[] slots)
    {
        var defined = slots.Select(s => new LogicalComputedValue(new LogicalLiteralExpression(true), s)).ToImmutableArray();
        return new LogicalCompute(new LogicalConstant(), defined);
    }
}
