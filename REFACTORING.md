# NQuery Major Refactoring

Perform these renames/moves. This makes it clearer where we are and allows us to
use "the right names" in the new universe:

    src\NQuery\Algebra               -> src\NQuery\Refactor\Algebra
    src\NQuery\AlgebraOptimization   -> src\NQuery\Refactor\Optimization
    src\NQuery\AlgebraBinding        -> src\NQuery\Refactor\Binding
    src\NQuery\Emit                  -> src\NQuery\Refactor\Emit
    src\NQuery\EmittedIterators      -> src\NQuery\Refactor\Iterators

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
* Missing
  - Subqueries in a non-inner join's ON (inner joins are handled by hoisting the
    subquery conjunct into a filter above the join)
  - Instantiating CTEs
  - Should Empty/Constant just be a node that can return a table of literals?
  - Look at the legacy optimizer and compare it against the new pipeline. What
    optimizations are we performing already and which ones do we need to port?
  - The binder is doing a lot of algebrization we could keep it separate and
    make sure the bound nodes are a shallow representation to the syntax, mapped
    1:1
  - Review naming of various constructs and check whether there a better
    industrial terms
* Representing AND and OR
    - Use N-ary AND and OR
    - Use NNF
* What properties do we need to track?
    - Like sort order?
    - And where would those be tracked? On the logical operator?
* Unique state
  - Keep track of which keys are unique
  - Add support for tables to declare unique combinations of keys
* Null state
  - Keep track of null state
  - Leverage null state from columns (`Nullable<T>`, nullable reference types)
* Sort state
  - Add supported for table symbols declaring a sort order
  - Keep track of sorted keys to avoid re-sorting
  - Add MergeJoin for cases where the input is pre-sorted
  - Pass sort keys down such that we can use it to choose a stream aggregate +
    sort over a hash aggregate
* Use fuzzing for the parser to find parsing bugs
* Use fuzzing for the query pipeline to find queries that fail 
* Use benchmarks to compare old vs new engine
* Use benchmarks to optimize the engine further (e.g. row buffer copies, boxing,
  slot representation)

## Missing

### Algebra (Bound → Logical)

- Subqueries inside an INNER join's ON are lowered: subquery-free conjuncts stay on
  the join (preserving hash-match eligibility), and a subquery-bearing conjunct is
  hoisted into a filter above the join, with its Apply correlated to the join's whole
  (left ++ right) output. Subqueries in a non-inner join's ON are still unlowered
  (moving the conjunct above the join changes outer-join semantics) and throw
  NotSupportedException. A bound join's Probe/PassthruPredicate are always null here
  (the binder never sets them; only the legacy SubqueryExpander does), so a bound-tree
  passthru can't arise — the algebrizer just asserts that invariant.
- CASE short-circuiting is honored: a subquery in a THEN/ELSE branch is evaluated only
  when that branch is selected. The algebrizer threads a passthru guard (the condition
  under which a branch isn't taken) through CASE branches and stamps it onto the Apply
  the subquery introduces (LogicalApply.Passthru). A guarded Apply is left as nested
  loops -- ApplyPushdown won't decorrelate it, and the planner passes the guard to
  PhysicalNestedLoops, whose iterators skip (pass through) the right for guarded rows.
  This is what keeps a multi-row subquery's cardinality assert from firing for a branch
  that's never taken.
- CTE cloning — the "non-trivial transform needs a pass" case we discussed; no
  instantiation pass exists yet (only trivial derived-table inlining is done). The
  slot-remapping machinery it needs is now available: LogicalOperatorCloner (built
  for the FULL OUTER expansion) deep-copies a logical subtree with fresh value slots.

### Planning (Logical → Physical)

- Any cost basis — no cardinality/cost estimation; join-algorithm and join-order
  choices are purely syntactic. (An equi-join inner/left-outer/full-outer becomes a
  hash match, everything else nested loops; there is no smaller-side build choice.)
- Aggregate strategy — always a stream aggregate: PhysicalStreamAggregates over a
  PhysicalSort on the grouping columns. No hash-aggregate alternative, and no use
  of existing ordering to elide the sort.
- Apply execution strategy — a LogicalApply lowers to a PhysicalNestedLoops
  carrying OuterReferences (the left columns the right reads); there is no separate
  physical/executable Apply node. It runs as naive correlated nested loops (re-scan
  the right per left row); no indexed-seek inner or spool/rewind.
- Sort elimination when input is already ordered (no ordering-property
  propagation into the planner), and no index/seek selection (table scan only).

### Emit (Physical → ExecutablePlan)

- All physical operators emit. (ExecutableConcatenation and ExecutableStreamAggregates
  are wired; ExecutableNestedLoops is wired and also serves Apply. INTERSECT/EXCEPT have
  no node of their own — the planner lowers them to a distinct sort + semi/anti-semi
  nested-loops join.)

### EmittedIterators

- The nested-loops family is ported (inner, left outer, left semi, left anti-semi,
  probing left semi) and serves joins, applies, and INTERSECT/EXCEPT (the latter lowered
  in the planner, so there is no dedicated set-difference iterator); a hash match
  (EmittedHashMatchIterator, inner/left-outer/full-outer over an equi-key), a stream
  aggregate (EmittedStreamAggregateIterator), and a concatenation
  (EmittedConcatenationIterator) are ported; the legacy NQuery.Iterators also has a table
  spool, deliberately not ported yet — it needs the compile-once treatment, not a copy.
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
  cross, left outer, probing semi via EXISTS / NOT EXISTS), correlated apply
  (a surviving TOP-1 scalar subquery), stream aggregates (scalar and grouped,
  including empty input and NULL grouping/argument handling), concatenation
  (UNION ALL and UNION, the latter via a distinct sort), INTERSECT/EXCEPT
  (including NULL-equals-NULL matching and multi-column predicates), scalar
  subqueries (the cardinality guard's assert firing on multi-row, passing on a
  unique-key single row, and skipped for a provably single-row aggregate),
  hash-match joins (inner/left-outer/full-outer over an equi-key, including a
  nullable key and a non-equi residual remainder), FULL OUTER JOIN (an equi
  condition goes to a hash match; a non-equi one the planner expands to left-outer
  UNION ALL right-anti-semi with the left null-padded, cloning the inputs with
  LogicalOperatorCloner so each branch is slot-disjoint), subqueries in an inner
  join's ON (EXISTS / NOT EXISTS / uncorrelated scalar aggregate), and CASE-branch
  subqueries guarded by passthru (a multi-row subquery in a never-taken THEN/ELSE is
  skipped rather than asserting, and a conditional guard still yields the right values).
- A hash match builds on the join's left and probes with its right (no smaller-side
  choice yet). Semi/anti joins and non-equi conditions stay nested loops. The next
  natural step is a merge join for pre-sorted inputs and cost-based build-side / join
  algorithm selection.