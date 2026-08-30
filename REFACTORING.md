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
* Functional dependency recognition. When a column set (e.g. a primary key)
  uniquely determines other columns, the optimizer can derive many things:
    - **`GROUP BY` omission**: you can `GROUP BY pk` and reference any column
      of that table in `SELECT` without listing it — PostgreSQL, MySQL (strict
      mode), and SQLite all support this; SQL Server and Oracle do not.
    - **`DISTINCT` elimination**: `SELECT DISTINCT pk, col FROM t` is redundant
      because pk is already unique; the `DISTINCT` can be dropped.
    - **Join elimination**: if a foreign-key references a unique key and no
      columns of the referenced table are needed, the join is removable.
    - **Cardinality estimation**: a unique join key guarantees no fan-out,
      affecting join order and algorithm selection.
    - **Scalar subquery guard**: when a subquery provably returns ≤1 row,
      the artificial `COUNT`/`ANY` safety guard can be omitted.
    Of these, SQLite only implements the `GROUP BY` omission — the rest are
    general optimizer capabilities that PostgreSQL supports more fully.
* Support `SELECT e FROM Employees e` — treating a table alias as a whole-row
  composite/entity reference rather than requiring explicit columns. This is
  valid in JPQL/HQL (`SELECT e FROM Employee e` returns `Employee` entity
  objects) and PostgreSQL (the alias refers to the row's composite type, usable
  in `ORDER BY`, function arguments, etc.). In NQuery this would mean the table
  alias is a valid expression in the SELECT list, yielding a runtime row value.
    - Once row-valued expressions exist, methods on rows follow naturally:
      `SELECT e.GetAge() FROM Employees e`. PostgreSQL supports this via
      composite-type function notation (`SELECT age(e) FROM Employees e` or
      `SELECT e.age FROM Employees e` when a matching function exists), and
      JPQL/HQL allows navigation to computed/derived properties on entities.

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

* **Constant folding and propagation.** The old engine ran `ConstantFolder`
  before and after algebrization; the new engine folds nothing (the only `Fold`
  code is aggregate folds). So `1+1`, `WHERE 1=0`, `NULL IS NULL`, etc. are never
  simplified. Highest value — but the value is entirely in what it *enables*, not
  in the folding itself: rewriting `1+1` to `2` saves one addition per row, while
  a predicate known to be FALSE lets us drop a conjunct, remove a filter, replace
  a subtree with an empty scan, and collapse the joins and unions above it (the
  next bullet). Reaching those rewrites takes both halves below; either one alone
  stops short.

  **Folding within an expression.** Evaluate what the query already spells out:
  operators, conversions, `IS NULL`, and CASE labels with a constant condition.

    - Not everything is foldable. `@p` isn't: a variable is read on every
      execution while `Query` compiles once and caches the plan, so folding it
      would freeze the first execution's parameters into every later one. (Which
      is why the old engine's `@p IS NULL` fold doesn't carry over; see
      *VariableDefinition* under *Miscellaneous*.) Nor are functions, methods, and
      properties — user-supplied members with no purity contract, where `RANDOM()`
      must not become a constant and an expensive or side-effecting member must
      not run at compile time.
    - Evaluation has to be failure-tolerant. A constant `1/0` folds to *nothing*,
      not to a compile-time error: the expression may never be reached at run time
      (an empty input, a CASE branch not taken), and folding must not turn a query
      that runs into one that fails to compile. Note this is the opposite of C#,
      where constant overflow and constant division by zero are errors.
    - Where to fold is a real choice. Roslyn's model is worth copying: rather than
      replacing the node, give each bound expression an optional constant value,
      computed during binding. That is non-destructive, so `SemanticModel` — which
      *is* the bound tree — keeps showing the user what they wrote, while the
      authoring layer gets trivial access to the value, to dead CASE branches, to
      always-false predicates. Only a small subset of expressions folds, so the
      rules stay small; here they can be smaller still, because the
      signature -> `System.Linq.Expressions` mapping in `ExpressionCompiler`
      (`BuildBinaryExpression`/`BuildUnaryExpression`) is pure signature lowering
      with nothing emit-specific in it. Moved next to the signature tables in
      `Binding`, the binder's folder and the emitter would share one definition of
      what each operator means, leaving only the three-valued NULL glue
      hand-written — and a differential test (folded value == executed value over
      a corpus) pins even that.
    - A logical-layer fold is still needed either way, for the constants that
      don't exist at bind time: the NULL padding an empty-scan collapse leaves
      behind, a probe slot forced to a constant, whatever propagation substitutes
      below. With the binder annotating and the algebrizer materializing, that
      pass keeps only the structural rules (conjunct dropping, CASE pruning,
      AND/OR identities).

  **Propagating across value slots.** Folding alone never reaches the interesting
  rewrites, because a constant does not cross a value slot: `Compute(Expr1 :=
  FALSE)` followed by `Filter(Expr1)` stays a per-row filter, since the filter
  sees a slot reference and not the literal. That is exactly the shape `WHERE
  EXISTS (SELECT ... WHERE 1=0)` lowers to, so without propagation the headline
  case never becomes an empty scan. Substituting a slot whose definition is a
  literal — and then re-folding — is what closes the loop.

    - It cannot be an annotation on `ValueSlot`. A slot is position-free identity:
      minted once, referenced from many places, so it can only carry what is true
      *everywhere*. "Is 1" isn't — under a LEFT JOIN the unmatched rows read that
      slot through a `NullRowBuffer` and see NULL, so a slot-level annotation would
      be believed in the one place it is false. Rule of thumb: annotate what has
      one position (an expression node), derive what has many (a slot, a relation).
    - So it is a derived property of an operator's output, in the same family as
      uniqueness, null state, and cardinality — computed from the children plus
      the operator's own semantics, with the constants of an outer join's
      null-supplied side dropped on the way *out* (its own `ON` condition may
      still use them, since that runs before any padding). See *What properties do
      we need to track?* under *Miscellaneous*: this wants the same lazily
      computed, per-operator plumbing, so building it alongside unique state is
      cheaper than building it twice.
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
* Providers receive a `CancellationToken` but only the base classes act on it,
  by passing it to `GetSemanticModel`. Nothing checks it between providers, so a
  fan-out over fifteen quick info providers still runs to completion after
  cancellation. Cheap to fix in the services' loops.
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
* Make the obvious way to ask for diagnostics the correct one
    - `SemanticModel.GetDiagnostics()` returns binding diagnostics only. A caller
      that wants everything also has to call `SyntaxTree.GetDiagnostics()`, and
      nothing in the API hints at that.
    - `Compilation` already combines both, but `GetDiagnostics(BindingResult)` is
      private, so the combined set isn't reachable from outside.
    - This is a pit of failure, not just an inconvenience: the LSP server
      reported no errors at all for an unterminated string literal, because that
      document has a lexer diagnostic and no binding diagnostics. It went
      unnoticed until a test happened to use a syntax-only invalid query.
    - Either expose a public combined `Compilation.GetDiagnostics()`, or have
      `SemanticModel.GetDiagnostics()` include the syntax diagnostics. The latter
      matches what Roslyn does and is what callers evidently expect.
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

## Shapes, attributes, and the metadata model

A design sketch for unifying the metadata model and reworking member discovery.
**Not yet implemented.** Supersedes `IPropertyProvider`/`IMethodProvider` and
folds in the TVF work (*Support table-valued functions*) and whole-row values
(*Support `SELECT e FROM Employees e`*).

The divide between tables/columns and values/properties is redundant, and
cross-cutting concerns (table-valued results, schema) end up modeled separately
in each place. `IPropertyProvider`/`IMethodProvider` are also all-or-nothing:
registering one seizes 100% of discovery for a type, with no seam for filtering,
renaming, adding synthetic members, or attaching schema.

### Core types

A **`Shape`** is the structure of a value — its access surface (what you dot
into) and, for a rowset, its columns. It holds *both* attributes and methods,
because a table row is just a value: `SELECT e.GetAge(), e FROM Employees e` dots
into and projects a row the same way as any value.

```csharp
// The structure of a value. Type + members.
sealed class Shape
{
    Type Type { get; }
    ImmutableArray<Attribute> Attributes { get; }   // 0-arg projections: property AND column, unified
    ImmutableArray<Method> Methods { get; }          // parameterized, overloadable
}

// A named projection from an instance to a value (property/column unified).
sealed class Attribute
{
    string Name { get; }
    Type Type { get; }            // result type; its Shape is resolved on demand, not held
    MemberInfo? Member { get; }   // provenance; null for synthetic (used by shaping conventions)
    Expression Access(Expression instance);
}

// A parameterized member of a shape (a Function with a receiver).
sealed class Method
{
    string Name { get; }
    Type ReturnType { get; }
    ImmutableArray<ParameterDefinition> Parameters { get; }
    Expression Invoke(Expression instance, IEnumerable<Expression> arguments);
}
```

`Table` / `Function` / `Variable` stay as the familiar top-level catalog
citizens — *not* collapsed into a single `Member` node (tried and rejected: too
abstract). Consumption modes: `SELECT e` projects the whole value; `SELECT *`
expands **attributes only**; `e.GetAge()` invokes a **method**. Table-valued
results need no flag — any attribute/method/function typed `IEnumerable<T>` is
usable in `FROM`/`APPLY`, row shape = the element type's shape.

### `TypeShaper` — the `Type → Shape` resolver

The bridge from CLR types into shape-space, consulted only at *leaves*. Injected
as a dependency (not owned by a mutable catalog); the `Catalog` composes one and
exposes it as `Shaper`. Definitions stay inert — `Create` resolves eagerly and
keeps only the resulting `Shape`.

```csharp
sealed class TypeShaper
{
    Shape Shape(Type type);                              // memoizing; explicit shape wins, else type-driven
    static TypeShaper Create(Action<ShapeConventionBuilder> configure);
}

abstract class TableDefinition
{
    Shape RowShape { get; }                              // resolved at Create time; RowType == RowShape.Type
    static TableDefinition Create<T>(string name, IEnumerable<T> src, TypeShaper shaper); // shaper.Shape(typeof(T))
    static TableDefinition Create(string name, IEnumerable src, Shape shape);             // explicit: DataRow, object[]
}
```

### Conventions replace the providers

One mechanism. A convention is a named delegate over a mutable builder — not an
interface, not a bare `Action`. Two kinds: **seeding** (produce elements — the
reflection walk is just the default first convention) and **shaping**
(filter/rename/annotate; uses `Attribute.Member` provenance, as the built-in
`[NQueryName]`/`[NQueryIgnore]`/`[NQuerySchema]` reader does).

```csharp
delegate void ShapeConvention(ShapeBuilder shape);

sealed class ShapeConventionBuilder            // starts empty; Build() -> TypeShaper
{
    ShapeConventionBuilder AddDefaultConventions();            // reflection(Public|Instance) + attribute reader
    ShapeConventionBuilder AddFields(BindingFlags f, Func<FieldInfo, bool>? where = null);
    ShapeConventionBuilder AddProperties(BindingFlags f, Func<PropertyInfo, bool>? where = null);
    ShapeConventionBuilder AddMethods(BindingFlags f, Func<MethodInfo, bool>? where = null);
    ShapeConventionBuilder Add(ShapeConvention convention);
    ShapeConventionBuilder Clear();
}

// "public fields, skip readonly, no properties" — no reflection defaults, so no properties surface:
TypeShaper.Create(b => b.AddFields(BindingFlags.Public | BindingFlags.Instance, f => !f.IsInitOnly));
```

### The hard part: shape must propagate, not be re-derived

"Shape is driven by the type" holds only at the *leaf*. Once a whole row is
projected (`SELECT e`) and its schema isn't recoverable from its CLR type — two
`DataTable`s share row type `DataRow` but have different columns — the type stops
carrying the schema:

```SQL
WITH LondonEmployees AS (SELECT e FROM SomeDataTable e WHERE e.City = 'London')
SELECT e.X FROM LondonEmployees      -- resolving X against typeof(DataRow) fails
```

So **`Shape` is the value's static query-type**, established at the leaf and
*propagated* by the query, never recomputed from the CLR type downstream. The
binder tracks a `Shape` (not just a `Type`) for anything dottable/projectable,
and member access resolves against `target.Shape` instead of today's
`LookupProperties(target.Type)`. For POCOs this is a no-op; for `DataRow` it is
what distinguishes two same-typed tables (one propagates `{X, Y}`, the other
`{A, B}`). Rule of thumb: **type-driven at the leaf, shape-propagated
thereafter.**

### Scope and touch points

Unifies the metadata model and member *visibility*, but does **not** merge the
binder's symbol roles (`FunctionSymbol`/`PropertySymbol`/`TableSymbol` still
differ — shape decides *what names are visible*, position decides *what a
reference compiles to*). In today's code: `Catalog` drops
`PropertyProviders`/`MethodProviders` for an injected `TypeShaper`;
`ColumnSymbol`/bound values gain a `Shape`; member-access binding reads the
carried shape; `SchemaTableSymbol` builds columns from a resolved/explicit
`Shape` rather than `TableDefinition.Columns`. Related: *VariableDefinition*
ownership and *TypeSymbol* hosting members lazily.
