# NQuery Major Refactoring

## Increase test coverage:

* A correctness test that runs in debug builds and asserts the bound tree is
  correct after each optimization step:
    - every referenced ValueSlot is either defined by a descendant or is a
      declared outer reference (this is the silent-wrong-result guard from our
      earlier discussion);
    - no slot is defined twice;
    - physical/logical node kinds appear where they're allowed.
* Per pass behavioral tests
* A small set (~a dozen) of curated snapshots for interesting queries which
  assert the full plan in approval testing style

## Remaining items

* Make sure that all plans are executable
* Should symbols refer to value slots at all?
    - No, we should remove that
    - Simplify the value slot assignments
    - Is `ValueSlot` a good term? Or should we go with `ColumnId`?
* Representing AND and OR
    - Use N-ary AND and OR
    - Use NNF
* What properties do we need to track?
    - Like sort order?
    - And where would those be tracked? On the logical operator?

## Missing

### Algebra (Bound → Logical)

- Subqueries inside JOIN ON / passthru conditions — explicitly throws
  NotSupportedException (Algebrizer.cs:145). An Apply attaches to one input, but
  a join condition sees both sides, so this case isn't lowered.
- CTE cloning — the "non-trivial transform needs a pass" case we discussed; no
  pass exists (only trivial derived-table inlining is done).

### Planning (Logical → Physical)

- Any cost basis — no cardinality/cost estimation; join-algorithm and join-order
  choices are purely syntactic.
- Aggregate strategy — always one PhysicalAggregate; no stream-vs-hash choice,
  no use of existing ordering.
- Apply execution strategy — no spool/rewind planning for correlated Apply;
  PhysicalApply is emitted verbatim.
- Sort elimination when input is already ordered (no ordering-property
  propagation into the planner), and no index/seek selection (table scan only).

### Emit (Physical → ExecutablePlan)

- ExecutableJoin, ExecutableApply, ExecutableAggregate, ExecutableConcatenation,
  ExecutableIntersectOrExcept.

### EmittedIterators

- No join/apply/aggregate/concatenation/intersect-except iterators. The legacy
  NQuery.Iterators has these (nested-loops family, hash match, stream aggregate,
  concatenation, table spool) but they were deliberately not ported — they need
  the compile-once treatment, not a copy.
- EmittedExpressionCompiler is single-buffer — it compiles against one
  operator's output-slot layout. Join/Apply predicates need a combined
  left+right slot map; CreateSlotIndices could build it, but nothing wires it
  yet.
- No spool/rewind iterator for correlated Apply.

### Cross-cutting

- The whole new pipeline is parallel and test-only — Query/QueryReader still run
  on the old IteratorBuilder/ExpressionBuilder. Nothing public routes through
  it.
- No ShowPlan/explain for physical or executable plans.
- End-to-end execution (the differential test vs. the existing engine) currently
  covers scan, filter, compute, project, sort, top — exactly the wired set
  above.
- The single biggest unblocking piece is ExecutableJoin + its emitted iterators
  (nested-loops and hash-match, with combined-buffer predicate compilation);
  Apply, Aggregate, and the set operators follow from the same machinery.