# NQuery Major Refactoring

* Ask Claude if the old unit tests did test something that we don't (unlikely)
* Do a manual, full review, of the entire refactoring.

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
* Support table-valued functions (TVFs)
    - This applies to functions as well as methods
    - The core building block would probably be `IEnumerable<T>`, potentially
      with `IQueryable<T>`. The schema would be inferred via `IPropertyProvider`
* Support tables over `IEnumerable<ValueTuple<...>>`
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
* Should we support table definitions backed by `IQueryable<T>`? What would it
  look like to support forwarding joins?
* It seems some function do work that would be worthwhile pre-computing, such as
  SOUNDS LIKE, LIKE, SIMILAR TO.

## Missing optimizations from the old engine

A pass-by-pass comparison of the old `Compilation` pipeline against the new
`LogicalOptimizer` / `Planner` turned up the optimizations below, segregated by
what they need. The **logical** group is doable now — each is a strict,
semantics-preserving win decidable from *provable* static facts (provably-empty,
provably-<=1-row, unique keys, constants), so none can regress. The **cost-based**
group needs cardinality *estimation* and is blocked on the cost model, which
neither engine has. Some overlap existing entries above and below — cross-referenced
where they do.

### Logical (no cost model needed)

* **Constant folding.** The old engine ran `ConstantFolder` before and after
  algebrization; the new engine folds nothing (the only `Fold` code is aggregate
  folds). So `1+1`, `WHERE 1=0`, `@p IS NULL` for a known parameter, etc. are never
  simplified. Highest value — and the prerequisite for empty-scan propagation:
  without folding, a contradiction predicate never becomes an empty input.
* **Null/empty-scan propagation** (old `NullScanOptimizer`). Collapse *provably*
  empty inputs up the tree: a contradiction predicate or empty leaf becomes an empty
  scan, then filters/joins/unions above it fold away. We have
  `LogicalEmpty`/`LogicalConstant` nodes but nothing *produces* `Empty` from a
  `WHERE 1=0` or propagates it. Pairs with constant folding — neither fires without
  the other. Subsumes the "Detect Filters/Join condition that are always false"
  bullet under *More features*.
* **Semi-join simplification** (old `SemiJoinSimplifier`). Inside a semi-join's right
  side only existence matters, so result/sort/distinct nodes there are redundant and
  can be stripped. No equivalent today.
* **Compute deferral** (the structural half of old `ComputationPusher`). Lift a
  `ComputeScalar` *up* past provably-restrictive operators (filters, semi/anti-joins)
  so it runs on the already-narrowed row set — unconditional, since those operators
  only shrink their input. (Pushing *down* to the leaves is the cost-based half; see
  below.) Complements the "Merge ComputeScalar() nodes" bullet under *More features*.
* **Outer-join reordering** (old `OuterJoinReorderer`) — *gated on unique-state.*
  Pull a LOJ out of an enclosing IJ (`(A LOJ B) IJ C -> (A IJ C) LOJ B` when the IJ
  does not depend on B); our join orderer only reorders contiguous inner-join regions,
  so an interposed outer join blocks reorderings the old engine could unblock. The two
  cost terms move oppositely: the C-probe is *unconditionally* cheaper reordered
  (original probes C over `A LOJ B`, `>= |A|` since a LOJ never drops an A row;
  reordered probes over just `A`), but the B-outer-join runs over `A IJ C`, so it
  blows up when C fans out. Net: a strict win iff the inner join is non-expansive —
  `C`'s join key unique, at most one match per A row. Absent unique-state a blind
  reorder can regress (C ×5, B 1:1 => 5x the outer-join work), so there it's only a
  cost-based orderer enabler.
* **At-most-one-row reordering** (old `AtMostOneRowReorderer`) — *gated on
  unique-state.* Drop `Top`/`Sort` when the input provably yields <=1 row, and reorder
  semi-joins over an at-most-one-row LOJ. "Provably <=1 row" is exact (from
  keys/uniqueness), not an estimate — hence logical. Needs the max-cardinality
  property; see the "Unique state" bullet under *Miscellaneous*, which already wants it
  for the scalar-subquery guard.

The last two both hinge on the same *provable* uniqueness / max-cardinality property.
Building it (the "Unique state" bullet) is itself logical work and unblocks both.

### Cost-based (blocked on cardinality estimation)

* **Compute placement to minimum cardinality** (the push-*down* half of old
  `ComputationPusher`). Pushing a `ComputeScalar` *below* a join pays only when the
  join fans out; below a filter or selective join it computes on discarded rows — a
  regression. The cheapest placement depends on the row count at each level, so it
  needs the cost model. The always-safe subset is the deferral direction listed under
  *Logical*.

See the broader "Cost model / cardinality estimation" item under *More features* and
the eager-spool follow-up under *Index spool follow-ups* — both blocked on the same
statistics.

Not worth copying: `OutputListGenerator`, `RowBufferEntryNamer`,
`FullOuterJoinExpander`, and `OuterReferenceLabeler` are bookkeeping for the old
row-buffer execution model that the value-slot / `Planner` design handles
structurally.

## More test coverage

* Make sure that all plans are executable
* Per pass behavioral tests
* A small set (~a dozen) of curated snapshots for interesting queries which
  assert the full plan in approval testing style

## Miscellaneous

* VariableDefinition
    - Seems a bit odd of have that live on the catalog. Logically that should
      probably be query owned.
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

## Index spool follow-ups

Remaining follow-ups:

* An eager spool (rebuild on outer change) would extend coverage to correlated
  inputs; nothing selects it today. **Blocked on the cost model, not a structural
  gap.** Unlike the probe hoist (a strict win, hence a logical rewrite), an eager
  spool can *regress*: its payoff is the rewind/rebind ratio -- how often the
  correlation value repeats between consecutive outer rows -- which depends on the
  value's distinct-count and the outer's ordering, neither visible from plan shape.
  Worst case (value changes every row, one probe per row) is all rebinds, no reuse:
  plain re-execution plus index-build overhead, strictly worse. Deciding it needs
  cardinality (compare `outer_rows x input_cost` vs `n_distinct x input_cost +
  probe`, plus a clustering sort). Declining it never regresses.
