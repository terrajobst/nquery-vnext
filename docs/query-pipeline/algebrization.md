# Algebrization

The **Algebrizer** (`NQuery.Refactor.Algebra.Algebrizer`) lowers the binder's syntax-shaped `BoundQuery` into a tree of **logical relational algebra operators** (`LogicalOperator`). This is the **Bound → Logical** phase.

## Entry point

```csharp
public static LogicalQuery Algebrize(BoundQuery query);
public static LogicalQuery Algebrize(BoundExpression expression); // wraps in one-row projection
```

## Responsibilities

1. **Relational pipeline construction**: A SELECT query's clauses (`FROM`, `WHERE`, `GROUP BY`, `HAVING`, `ORDER BY`, `TOP`, `SELECT`) are each lowered into the appropriate relational operator in the correct order.
2. **Subquery lowering**: Scalar subqueries become `LeftOuter Apply` operators whose result slot is referenced by the enclosing expression. `EXISTS` subqueries become `Semi Apply` operators producing a boolean probe slot. Correlation is made explicit at this stage — the logical expression language contains no query nodes.
3. **CASE passthru guards**: Subqueries inside `CASE` branches get a `LogicalApply.Passthru` guard so they are conditionally skipped when the branch is not taken.

## Operator properties

All logical operators are **immutable**. They lazily compute and cache two sets:

- **DefinedValueSlots**: The set of slots the operator introduces (used for membership tests — "does this subtree define that slot?").
- **OutputValueSlots**: The ordered array of slots visible on the operator's output (the column order of the result).

```csharp
internal abstract class LogicalOperator
{
    public abstract LogicalOperatorKind Kind { get; }
    public FrozenSet<ValueSlot> DefinedValueSlots { get; }
    public ImmutableArray<ValueSlot> OutputValueSlots { get; }

    protected abstract FrozenSet<ValueSlot> ComputeDefinedValueSlots();
    protected abstract ImmutableArray<ValueSlot> ComputeOutputValueSlots();
}
```

Helpers: `LogicalOperatorCloner` (deep clone with slot remapping) and `LogicalOperatorRewriter` (visitor/replacement base class).

## Type Hierarchy

All logical operators inherit from `LogicalOperator`:

```
LogicalOperator
├── LogicalEmpty              — zero rows
├── LogicalConstant           — single row with constant values
├── LogicalTableScan          — scans a table, emits column value slots
├── LogicalFilter             — filters rows by predicate
├── LogicalCompute            — computes new value slots
├── LogicalProject            — passes a subset of slots (optionally distinct)
├── LogicalJoin               — inner, outer, cross, semi/anti-semi joins
├── LogicalApply              — correlated join with passthru guard
├── LogicalAggregate          — group-by and scalar aggregation
├── LogicalUnion              — concatenates rows from two inputs
├── LogicalIntersectOrExcept  — set intersection or difference
├── LogicalSort               — orders rows by key columns
├── LogicalTop                — limits rows (with optional ties)
└── LogicalAssert             — validates a condition
```
