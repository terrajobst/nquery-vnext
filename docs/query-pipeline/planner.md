# Planner

The **Planner** (`NQuery.Planning.Planner`) translates the optimized logical algebra into a **physical operator tree** (`PhysicalOperator`). This is the **Logical → Physical** phase.

## Entry point

```csharp
public static PhysicalQuery Plan(LogicalQuery query);
public static PhysicalOperator Plan(LogicalOperator root);
```

## Type Hierarchy

All physical operators inherit from `PhysicalOperator`:

```
PhysicalOperator
├── PhysicalEmpty              — zero rows
├── PhysicalConstant           — single-row constant
├── PhysicalTableScan          — table scan with column accessors
├── PhysicalFilter             — filters rows by predicate
├── PhysicalComputeScalar      — computes value slots from expressions
├── PhysicalProject            — passes through a subset of slots
├── PhysicalNestedLoops        — nested-loops join (inner, outer, semi, anti)
├── PhysicalHashMatch          — hash equi-join (inner, left/right/full outer)
├── PhysicalStreamAggregates   — stream aggregation (sorted input)
├── PhysicalSort               — sorts rows by key columns
├── PhysicalTop                — limits rows (with optional ties)
├── PhysicalConcatenation      — concatenates rows from multiple inputs
└── PhysicalAssert             — validates a condition
```

## Current approach

The planner currently performs a **one-to-one lowering**: every logical operator maps to a single physical operator. Algorithm selection (e.g., hash join vs. nested loops for equi-joins, stream vs. hash aggregation) and cost-based optimization are not yet implemented — all joins default to nested loops.

The physical plan is the last tree representation before code generation. It is the target of the `PhysicalShowPlanBuilder` for execution plan visualization.
