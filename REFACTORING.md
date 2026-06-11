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
* Instantiating CTEs
    - Need to handle recursive ones
* Not implemented — these nodes build but throw NotSupportedException from CreateIterator():
  - ExecutableAggregate
    - HashAggregate
    - StreamAggregate
  - ExecutableConcatenation    
  - ExecutableIntersectOrExcept
* Cases
  - Distinct
  - Distinct with ties
  - Union
  - Except
  - Intersect
  - Full outer join
* Subqueries in join conditions
    - Is that where passthru comes from?
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
- Apply execution strategy — a LogicalApply lowers to a *dependent*
  PhysicalNestedLoops (IsDependent = true); there is no separate physical/
  executable Apply node. It runs as naive correlated nested loops (re-scan the
  right per left row); no indexed-seek inner or spool/rewind.
- Sort elimination when input is already ordered (no ordering-property
  propagation into the planner), and no index/seek selection (table scan only).

### Emit (Physical → ExecutablePlan)

- ExecutableAggregate, ExecutableConcatenation, ExecutableIntersectOrExcept.
  (ExecutableNestedLoops is wired and also serves Apply.)

### EmittedIterators

- No aggregate/concatenation/intersect-except iterators, and no hash match. The
  nested-loops family is ported (inner, left outer, left semi, left anti-semi,
  probing left semi) and serves both joins and applies; the legacy
  NQuery.Iterators also has hash match, stream aggregate, concatenation, and table
  spool, deliberately not ported yet — they need the compile-once treatment, not a
  copy.
- ExecutableNestedLoops compiles its predicates against the combined (left ++
  right) slot map via CreateSlotIndices. A dependent (apply) nested loops uses the
  same combined-buffer trick for correlation — its right subtree's filters/computes
  compile against (outer ++ input) and read an outer buffer threaded through
  CreateIterator.
- No spool/rewind iterator for correlated Apply (naive re-scan only). Join
  predicates that reference outer slots (correlation on a join, not a filter) are
  not handled — decorrelation routes correlation through filters.

### Cross-cutting

- The whole new pipeline is parallel and test-only — Query/QueryReader still run
  on the old IteratorBuilder/ExpressionBuilder. Nothing public routes through
  it.
- No ShowPlan/explain for physical or executable plans.
- End-to-end execution (the differential test vs. the existing engine) currently
  covers scan, filter, compute, project, sort, top, nested-loops joins (inner,
  cross, left outer, probing semi via EXISTS / NOT EXISTS), and correlated apply
  (a surviving TOP-1 scalar subquery).
- The next pieces are ExecutableAggregate, then the set operators and a
  hash-match join node; all reuse the combined-buffer predicate compilation that
  ExecutableNestedLoops established.