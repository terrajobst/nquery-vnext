# Executable (Emit)

The **Emitter** (`NQuery.Refactor.Emit.Emitter`) lowers the physical operator tree into an **executable plan** (`ExecutablePlan`) — a tree of `ExecutableOperator` instances ready to produce runtime iterators. This is the **Physical → Executable** phase.

## Entry point

```csharp
public static ExecutablePlan Emit(PhysicalQuery query);
```

## Key responsibility: compile-once expressions

All filter predicates, computed column expressions, sort keys, and join conditions are **compiled to delegates** at emit time. The `ExpressionCompiler` compiles NQuery expressions into LINQ `Expression<Tree>` instances and then into `Func<RowBuffer, object>` delegates. Because these delegates take the row buffer as a parameter, they are reusable across all rows and across multiple executions.

All expressions are compiled once at emit time rather than per-row at execution time.

## Outer slots

The emitter carries an `outerSlots` parameter through the recursive lowering. For operators inside an Apply's right side, this array contains the slots defined by the left side that the right side references. The emitter uses this information to compile correlated filters and computations against the combined (outer ++ input) row buffer layout.

```csharp
private static ExecutableOperator EmitOperator(PhysicalOperator node, ImmutableArray<ValueSlot> outerSlots);
```

## Type Hierarchy

All executable operators inherit from `ExecutableOperator`:

```
ExecutableOperator
├── ExecutableEmpty
├── ExecutableConstant
├── ExecutableTableScan
├── ExecutableFilter
├── ExecutableComputeScalar
├── ExecutableProject
├── ExecutableNestedLoops
├── ExecutableHashMatch
├── ExecutableStreamAggregates
├── ExecutableSort
├── ExecutableTop
├── ExecutableConcatenation
└── ExecutableAssert
```

## Result

The `ExecutablePlan` wraps the root `ExecutableOperator` and the query's output column symbols. It exposes a single method:

```csharp
public sealed class ExecutablePlan
{
    public Iterator CreateIterator();
}
```

Calling `CreateIterator()` instantiates the runtime iterator tree by walking the executable operator tree and constructing the corresponding stateful iterator objects. The compiled expression delegates are passed into the iterators' constructors.
