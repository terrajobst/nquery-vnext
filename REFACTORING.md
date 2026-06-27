# NQuery Major Refactoring

* We should use Ordinal for all string comparisons
* Ask Claude if the old unit tests did test something that we don't (unlikely)

## Port Common Table Expressions

The new binder fully binds CTEs (BindCommonTableExpressionQuery, recursive
validation via RecursiveCommonTableExpressionChecker), but nothing instantiates
them downstream. A CTE reference becomes a BoundNamedTableReference →
Algebrizer.AlgebrizeNamedTableReference → a plain LogicalTableScan over the
CommonTableExpressionSymbol. Then Emitter.EmitTableScan does
(SchemaTableSymbol)node.TableInstance.Table — which throws for a CTE-backed
instance. There is no equivalent of legacy CommonTableExpressionInstantiator.
The AlgebrizerTests CTE case (line 27) only asserts the logical shape, never
execution; no evaluation test drives a CTE through the new engine.

- Missing iterators: TableSpoolIterator, TableSpoolRefIterator, TableSpoolStack
  (the recursive-CTE execution machinery) have no Emitted* counterparts.
- Recursive CTEs are therefore unsupported end-to-end.

## More features

* Add support for `VALUES()`
    - We probably want derived table column syntax (like we do for CTEs) first
    - We probably want to share the syntax / machinery with CTE
    - `SELECT * FROM (VALUES (1, 'Immo'), (2, 'Thomas')) AS D` should be
      rejected ("column name required")
    - `SELECT * FROM (SELECT 1, 2, 3) AS D` should be rejected ("column name
      required")
    - It's worth noting that no mainstream SQL engine has the notion of unnamed
      columns for CTE/derived tables, only for the root query
* Make LogicalConstant hold a static table and use it cases like this:
    ```SQL
    SELECT 1, 2, 3
    UNION ALL
    SELECT 4, 5, 6
    UNION ALL
    SELECT 7, 8, 9
    ```
* Merge ComputeScalar() nodes
* Detect Filters/Join condition that are always false
* Cost model / cardinality estimation.
    - Planner is a one-to-one lowering: hash-match build side is always the
      join's left, and hash-vs-loops is structural (equi-key present), not
      cost-based.
    - We can have an extension point for providing statistics or even
      compute/sample our own during compilation, maybe a behavior that can be
      turned of entire or by table.
* Target typed enums
    - Support enums without registering a `TypeName` variable per type or adding
      static type members, using a leading-dot implicit-member syntax
      (Swift-style) such as `FIND(x.Name, .FromStart)` or `WHERE x.Option =
      .FromStart`.
    - The leading dot makes the expression unambiguously target-typed, so it can
      never collide with a column or variable name — no shadowing rules and no
      risk of a query silently changing meaning. No enum registry is needed: the
      target type (a function parameter, or the other side of a comparison)
      names the enum, and `.FromStart` is validated against its members.
    - Implementation-wise the lexer already emits `DotToken` for a leading dot
      (so `.5` floats are unaffected) and only one case is added to
      `ParsePrimaryExpression`; the harder part is binding, which must invert
      NQuery's bottom-up flow: the bare `.member` has no intrinsic type (like
      `NULL`), so it becomes a deferred candidate node that overload resolution
      leaves applicable to any enum parameter defining that member, then
      `BindArgument` rewrites it to a literal once an overload is selected. The
      comparison case (`= .FromStart`, `IN`, `CASE`) additionally requires
      `BindBinaryExpression` to push the concrete enum operand's type into the
      candidate, since binary operators don't target-type their operands by
      default.

## Missing optimizations from the old engine

A pass-by-pass comparison of the old `Compilation` pipeline against the new
`LogicalOptimizer` / `Planner` turned up seven optimizations we don't have. They
are listed roughly in priority order; the first three are the highest value.
Some overlap existing entries above and below — cross-referenced where they do.

1. **Constant folding.** The old engine ran `ConstantFolder` both before and
   after algebrization; the new engine folds nothing (the only `Fold` code is
   aggregate folds). So `1+1`, `WHERE 1=0`, `@p IS NULL` for a known parameter,
   etc. are never simplified. Beyond the direct win, this is a prerequisite for
   (2): without folding, a contradiction predicate never becomes an empty input.
2. **Null/empty-scan propagation** (old `NullScanOptimizer`). Collapse provably
   empty inputs up the tree: a contradiction predicate or empty leaf becomes an
   empty scan, then filters/joins/unions above it fold away. We have
   `LogicalEmpty`/`LogicalConstant` nodes but nothing *produces* `Empty` from a
   `WHERE 1=0` or propagates it. Pairs with (1) — neither fires without the
   other. Subsumes the "Detect Filters/Join condition that are always false"
   bullet under *More features*.
3. **Compute pushdown** (old `ComputationPusher`). Push `ComputeScalar` *down*
   past joins/filters so expressions evaluate on the smallest row set, closest to
   the leaves. Our `ProjectMerger` only collapses/removes projects — it never
   relocates a compute to reduce how many rows it runs over. Clearest pure-runtime
   win; complements the "Merge ComputeScalar() nodes" bullet under *More features*.
4. **Semi-join simplification** (old `SemiJoinSimplifier`). Inside a semi-join's
   right side only existence matters, so result/sort/distinct nodes there are
   redundant and can be stripped. No equivalent today.
5. **Outer-join reordering** (old `OuterJoinReorderer`). Pull a LOJ out of an
   enclosing IJ (`(A LOJ B) IJ C -> (A IJ C) LOJ B` when the IJ does not depend
   on B), exposing more inner-join orderings to `JoinOrderer`. Our join orderer
   only reorders contiguous inner-join regions, so an interposed outer join blocks
   reorderings the old engine could unblock.
6. **At-most-one-row reordering** (old `AtMostOneRowReorderer`). Drop `Top`/`Sort`
   when the input provably yields <=1 row, and reorder semi-joins over an
   at-most-one-row LOJ. Needs a max-cardinality query property — see the "Unique
   state" bullet under *Miscellaneous*, which already wants the same property for
   the scalar-subquery guard.
7. **W** (old `SpoolInserter`). Our
   strategy is to decorrelate via `ApplyPushdown`, which is better when it works;
   but a `LogicalApply` that survives decorrelation is lowered to plain nested
   loops that re-execute the inner subtree per outer row. Already captured in
   detail by "Port the index spool for correlated subqueries" under
   *Miscellaneous* (with the lazy-vs-eager correctness caveat); listed here for
   completeness.

Not worth copying: `OutputListGenerator`, `RowBufferEntryNamer`,
`FullOuterJoinExpander`, and `OuterReferenceLabeler` are bookkeeping for the old
row-buffer execution model that the value-slot / `Planner` design handles
structurally. The one thing *neither* engine does is a cost model — see "Cost
model / cardinality estimation" under *More features*.

## More test coverage

* Make sure that all plans are executable
* Add more correlation tests exploiting the new `CROSS APPLY` and `OUTER APPLY`
  to reach more cases
* Per pass behavioral tests
* A small set (~a dozen) of curated snapshots for interesting queries which
  assert the full plan in approval testing style

## Miscellaneous

* Make sure we have argument validation for all public APIs
    - For internal/private APIs I want argument validation for public statics and
      constructors
* Representing AND and OR
    - Use N-ary AND and OR
    - Use NNF
* What properties do we need to track?
    - Like sort order?
    - And where would those be tracked? On the logical operator?
* Unique state
    - Keep track of which keys are unique
    - Add support for tables to declare unique combinations of keys
    - Once we track max-cardinality/at-most-one-row as a query property, fold the
      scalar-subquery guard into it: the binder eagerly creates the Any/Count
      guard aggregates on every BoundSingleRowSubselect, but the algebrizer
      discards them whenever ReturnsAtMostOneRow holds. With a shared cardinality
      property both the binder's guard decision and Algebrizer.ReturnsAtMostOneRow
      read the same source, so the aggregates can become nullable and be created
      only when actually needed.
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
* Port the index spool for correlated subqueries
    - The Northwind `NestedLoops` benchmark (a correlated scalar `SELECT TOP 1 ...
      ORDER BY` per customer) is the one shape where the old engine crushes us:
      ~76us / 61KB vs the refactored engine's ~1.6ms / 506KB (and baseline's
      ~3.2ms / 1.9MB). See `docs/benchmarks.md`.
    - When a subquery has no decorrelation rule it survives as a dependent join,
      so we re-run the inner `Top -> Sort -> Filter -> Scan` for *every* outer row:
      O(outer x inner). The old engine instead recognizes that the correlated
      filter predicate is an equality between an inner column and an outer
      reference (`o.CustomerID = c.CustomerID`) and rewrites the per-row `Filter`
      into an index spool (`Compilation/SpoolInserter.cs` +
      `Execution Plan/IndexSpoolIterator.cs`): it scans the inner input *once*,
      builds a `Dictionary<key, List<row>>` keyed by the inner column, and then
      probes it by the outer value on each iteration. That turns the cost into
      O(inner + outer) and is essentially SQL Server's lazy/eager spool.
    - Correctness caveat the old engine already flagged (the `// TODO` at the top
      of `SpoolInserter.cs`): the cached spool is only valid when the spooled
      *input* does not itself depend on an outer reference. If it does, the spool
      must be invalidated whenever the outer reference changes (an eager spool),
      otherwise it returns stale rows. The old code does not distinguish lazy from
      eager spools, so a naive port would be a correctness bug; we should only
      build the persistent spool when the input is independent of all outer
      references, and fall back (or rebuild) otherwise.
* Use new C# language features
* Change the authoring to have a root-object that we can add language services
  to via extension methods. Maybe a WorkspaceBuilder?
* TypeSymbol
    - Support type aliases
    - Host methods and properties on TypeSymbol, lazily loaded.
* We still need to support cases where the type isn't known until runtime. That
  should probably be `compilation.CompileExpression(typeof(SomeType))` that
  returns `Expression<object>`.
* Add a formatter
    - Should use some standard SQL formatting rules
    - Should probably handle long lines
    - Should probably handle keyword casing
    - Should probably offer identifier normalization (brackets, quotes, always)
* Are properties on the show plan used at all?
* Add an LSP and add a VS Code plugin that replaces the VS based editor
  experience (we should keep the Actipro one though)
* InstantiatedAggregateSymbol. Today it's conceptually an open generic. We don't
  have a symbol that captures the instantiated aggregate. We should consider
  adding one (and have it implement IInvocableSymbol) and have quick info show
  the signature. Maybe we can simplify this using a Roslyn style API that
  collapses generic and instantiated generics into a single ITypeSymbol.
* What are standard SQL types and how should they map to our primitives?

## CTE Design

This is the concrete design for finishing CTE support; it supersedes the
sketch under *Port Common Table Expressions* above. Binding, validation, and
authoring are already complete (`BindCommonTableExpressionQuery`,
`RecursiveCommonTableExpressionChecker`, `CommonTableExpressionSymbol` carrying
`Anchor` + `RecursiveMembers`). Everything below is about *consuming* that from
the algebrizer down. Today a CTE reference falls through to a plain
`LogicalTableScan` and `Emitter.EmitTableScan` throws on the
`(SchemaTableSymbol)` cast.

### Non-recursive CTEs: inline at algebrize time

Treat a non-recursive CTE reference like a derived table, but instantiate the
anchor's logical subtree *per reference* with fresh value slots using the
existing `LogicalOperatorCloner` (its doc comment already anticipates this).
This is deliberately **not** a barrier: inlining lets the optimizer specialize
each copy and push predicates into the body. A single reference needs no clone;
each additional reference gets a slot-disjoint clone.

### Recursive CTEs: a monolithic, opaque logical node lowered in the planner

Represent recursion as a single opaque logical operator that survives binding,
algebra, and optimization unchanged:

* `LogicalRecursiveUnion` - owns the anchor subtree, the recursive-member
  subtree(s), the unified output slots, and a recursion identity token naming
  the canonical working-table slots.
* `LogicalRecursiveReference` - the self-reference leaf (the working table).

Keeping it opaque gives the **optimizer barrier for free**: no pass needs to
learn to "skip a spool region." The optimizer still recurses *into* the anchor
and member subtrees and optimizes them normally; it just never moves anything
*across* the boundary or decorrelates the reference. The reference leaf must
read like a non-empty table scan so empty/constant propagation does not fold it
away.

Lower `LogicalRecursiveUnion` in the **Planner**, exactly as `LogicalJoin`
lowers to `{PhysicalHashMatch | PhysicalNestedLoops}` and `LogicalApply` lowers
to a decorrelated join or nested loops. This decouples representation from
execution strategy and is where the index spool could be selected later.

### Execution model: working table (breadth-first), not stack spool

Two strategies are possible:

* **Stack spool / depth-first / row-at-a-time** - the legacy
  (`nquery-baseline`) approach: `Concat` + correlated nested loops + a
  `TableSpoolStack` shared by a push iterator and a pop leaf. The recursive
  reference is a *single row*, so the recursive join is *always* correlated
  nested loops.
* **Working table / breadth-first / set-at-a-time** - the recursive reference
  is the *whole current frontier* as a scannable set, so the recursive join is
  an ordinary join the planner can satisfy with a hash match.

Choose the **working table** model:

* Set-at-a-time exposes the recursive reference as a relation, so the normal
  join planner picks a hash join for the recursive step. The stack-spool model
  forecloses that.
* NQuery has **no indexes**, so the stack spool's per-row re-scan of the base
  relation has no index-seek rescue (the way SQL Server's does) - a hierarchy
  walk degrades to O(N^2). The working table does one base scan per round and a
  hash join against the frontier: O(D*N).
* The industry mainstream (PostgreSQL, MySQL, Oracle's recursive `WITH`,
  SQLite) all use the working-table/breadth-first model; SQL Server is the
  outlier that relies on index seeks.
* "Let the optimizer choose" is achieved **not** by maintaining two recursion
  drivers (no mainstream engine does - a fixpoint has no cardinality estimate
  to cost), but by one set-at-a-time driver plus the existing join planner
  choosing the recursive step's join (hash / nested-loops / future index spool).

Keep the stack-spool / index-spool as a possible future *physical*
specialization for the narrow correlated-re-seek case; do not commit the
representation to it.

### Iterators

* `RecursionIterator` - emits anchor rows (staging them as the round-0
  frontier), then ping-pongs two `SpooledRowStore`s (working / next): each round
  re-opens the recursive body over the working store, appends produced rows to
  the next store and emits them, then swaps and clears; stops when a round
  produces no rows. `MAXRECURSION` (default 100) is a per-round counter.
* `RecursiveReferenceIterator` - snapshots the current working store on `Open`,
  scans `0..Count`. It is effectively a table scan over a frozen set.
* `RecursiveWorkTable` - the one small shared-by-reference handle connecting the
  driver and the reference leaf (unavoidable: the leaf lives in a different
  subtree).

Breadth-first is markedly simpler in the demand-pull iterator model: the
reference is a scan, the driver is a two-store swap, and the recursion level is
just the round number - no bespoke stack, no per-row body re-open, no
materialize-to-push ordering.

### Traversal order caveat

Breadth-first changes output row order versus the old engine's depth-first.
This is standards-conformant (no order is guaranteed without `ORDER BY`), but
the `nquery-old` CTE evaluation corpus reflects depth-first ordering - port
those tests with an explicit `ORDER BY`, or accept the order difference.

### Cloning the recursion node

`LogicalRecursiveReference` is a **back-edge** to its `LogicalRecursiveUnion`,
so the recursive-member subtree is a graph, not a tree, and a structural
deep-copy desyncs (the bottom-up clone visits the descendant reference before
the ancestor union exists). Use **identity-based cloning**: both the union and
every reference hold a shared recursion token. When cloning a
`LogicalRecursiveUnion`, *pre-seed* the clone scope - mint the new token and
fresh canonical working-table slots and register old->new - **before** cloning
children, so every back-edge encountered during the descent rewires to the
clone. Invariant: a union and all its references are cloned together as one
slot-scope unit; assert that a reference's token resolves within scope. Extend
`LogicalOperatorCloner` with a recursion-token map and a special-cased
`CloneRecursiveUnion`.

### Reuse vs. new infrastructure

Reuse: all syntax/binding/validation; `CommonTableExpressionSymbol.Anchor` /
`RecursiveMembers`; `LogicalOperatorCloner`; `LogicalUnion` ->
`ConcatenationIterator`; `SpooledRowStore`; `LogicalApply`/`LogicalJoin` ->
nested loops; `LogicalCompute`; `LogicalAssert`.

New: `LogicalRecursiveUnion` / `LogicalRecursiveReference` (+ kinds, rewriter /
cloner hooks, show plan); their `Physical*` counterparts (+ `Planner`,
`PhysicalPlanVerifier`, show plan); their `Executable*` counterparts + `Emitter`
branches; `RecursionIterator` + `RecursiveReferenceIterator` +
`RecursiveWorkTable`; a defensive guard on the `EmitTableScan`
`SchemaTableSymbol` cast.

### Phasing

1. Non-recursive, single reference - unblocks the `AlgebrizerTests` CTE case and
   makes a CTE *execute* end-to-end.
2. Non-recursive, multiple references - clone per reference.
3. Recursive (working-table model + iterators).

Add evaluation tests throughout (none exist today): port the `nquery-old` CTE
corpus and compare each CTE's output against the hand-inlined equivalent.

### Open questions / cleanups

* **`CommonTableExpressionSymbol` construction.** The constructor leaks a
  partially-constructed `this` into binder callbacks and encodes an implicit,
  load-bearing ordering between two callbacks (anchor must set `Columns` before
  the recursive members bind). Collapse to a single callback returning
  `(Anchor, Members, Columns)`, or move orchestration into the binder with an
  explicit two-phase `Complete`.
* **Syntactic recursion detection.** `IsRecursive` is a sound over-approximation
  but decides a *binding* property *syntactically, pre-binding*: a name that
  merely collides with a base table (or an inner-scope table) is reported as
  malformed recursion ("no UNION ALL") rather than what it is. This is faithful
  to the SQL Server dialect (same behavior and message) but diverges from
  ANSI/PostgreSQL, where `RECURSIVE` gates self-visibility. A more precise design
  classifies a member as recursive iff a reference actually *resolves to* the
  CTE symbol (post-binding) - worthwhile only if dialect compatibility or
  diagnostic quality becomes a goal.
* **Benchmark** deep vs. wide hierarchies to validate the working-table choice,
  and ensure the recursive join builds its hash on the (small) frontier, not the
  base relation.
