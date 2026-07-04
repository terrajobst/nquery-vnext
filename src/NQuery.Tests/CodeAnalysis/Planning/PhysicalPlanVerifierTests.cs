using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Planning;

namespace NQuery.Tests.CodeAnalysis.Planning;

// The physical-plan verifier runs (in DEBUG) between Plan and Emit; every query the
// rest of the suite compiles exercises its happy path. These tests pin the failure
// path: a plan that references a slot the emitter could not resolve must be rejected
// with a clear error rather than surfacing as a slot-lookup failure deep in emit.
public class PhysicalPlanVerifierTests
{
    [Fact]
    public void PhysicalPlanVerifier_Accepts_WellFormedPlan()
    {
        // A filter whose condition references a slot its input actually produces.
        var factory = new ValueSlotFactory();
        var slot = factory.CreateNamed("c", typeof(bool));
        var input = Leaf(slot);
        var filter = new PhysicalFilter(input, [new LogicalValueSlotExpression(slot)]);
        var query = new PhysicalQuery(filter, []);

        PhysicalPlanVerifier.Verify(query);
    }

    [Fact]
    public void PhysicalPlanVerifier_Rejects_FilterReferencingUnavailableSlot()
    {
        // The condition references a slot that nothing below produces and no enclosing
        // apply supplies -- exactly the shape that used to crash the emitter.
        var factory = new ValueSlotFactory();
        var ghost = factory.CreateNamed("ghost", typeof(bool));
        var input = new PhysicalConstant();
        var filter = new PhysicalFilter(input, [new LogicalValueSlotExpression(ghost)]);
        var query = new PhysicalQuery(filter, []);

        var exception = Assert.Throws<InvalidOperationException>(() => PhysicalPlanVerifier.Verify(query));
        Assert.Contains(ghost.Name, exception.Message);
    }

    [Fact]
    public void PhysicalPlanVerifier_Accepts_OuterReferenceSuppliedByApply()
    {
        // The right side references a left slot, declared as the apply's outer reference --
        // the legitimate correlation the CROSS APPLY fix relies on.
        var factory = new ValueSlotFactory();
        var leftSlot = factory.CreateNamed("l", typeof(bool));
        var rightSlot = factory.CreateNamed("r", typeof(bool));

        var left = Leaf(leftSlot);
        var right = new PhysicalFilter(Leaf(rightSlot), [new LogicalValueSlotExpression(leftSlot)]);
        var apply = new PhysicalNestedLoops(PhysicalJoinKind.Inner, left, right, [], probe: null, passthruPredicate: null, [leftSlot]);
        var query = new PhysicalQuery(apply, []);

        PhysicalPlanVerifier.Verify(query);
    }

    [Fact]
    public void PhysicalPlanVerifier_Rejects_OuterReferenceNotInLeft()
    {
        // The apply claims to expose a slot that its left side does not produce.
        var factory = new ValueSlotFactory();
        var leftSlot = factory.CreateNamed("l", typeof(bool));
        var ghost = factory.CreateNamed("ghost", typeof(bool));

        var left = Leaf(leftSlot);
        var right = Leaf(factory.CreateNamed("r", typeof(bool)));
        var apply = new PhysicalNestedLoops(PhysicalJoinKind.Inner, left, right, [], probe: null, passthruPredicate: null, [ghost]);
        var query = new PhysicalQuery(apply, []);

        var exception = Assert.Throws<InvalidOperationException>(() => PhysicalPlanVerifier.Verify(query));
        Assert.Contains(ghost.Name, exception.Message);
    }

    [Fact]
    public void PhysicalPlanVerifier_Rejects_SlotIntroducedTwice()
    {
        // The same slot minted on both sides of a join -- the shape a rewrite that
        // duplicated a subtree without re-minting its slots would produce. Each side is
        // well-scoped on its own, so only the uniqueness check catches it.
        var factory = new ValueSlotFactory();
        var slot = factory.CreateNamed("dup", typeof(bool));
        var join = new PhysicalNestedLoops(PhysicalJoinKind.Inner, Leaf(slot), Leaf(slot), [], probe: null, passthruPredicate: null, []);
        var query = new PhysicalQuery(join, []);

        var exception = Assert.Throws<InvalidOperationException>(() => PhysicalPlanVerifier.Verify(query));
        Assert.Contains(slot.Name, exception.Message);
    }

    [Fact]
    public void PhysicalPlanVerifier_Accepts_SlotThreadedThroughManyOperators()
    {
        // A slot defined once at the leaf and merely passed up through Filter/Project is
        // not a double definition: only the leaf introduces it.
        var factory = new ValueSlotFactory();
        var slot = factory.CreateNamed("c", typeof(bool));
        var filter = new PhysicalFilter(Leaf(slot), [new LogicalValueSlotExpression(slot)]);
        var project = new PhysicalProject(filter, [slot]);
        var query = new PhysicalQuery(project, []);

        PhysicalPlanVerifier.Verify(query);
    }

    // A minimal leaf operator that outputs exactly the given slots: a compute of a
    // literal per slot over a one-row constant. The verifier only inspects slot
    // identity, so the literal's value is irrelevant.
    private static PhysicalOperator Leaf(params ValueSlot[] slots)
    {
        var defined = slots.Select(s => new LogicalComputedValue(new LogicalLiteralExpression(true), s)).ToImmutableArray();
        return new PhysicalComputeScalar(new PhysicalConstant(), defined);
    }
}
