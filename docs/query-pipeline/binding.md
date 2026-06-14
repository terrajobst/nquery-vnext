# Binding

The **Binder** (`NQuery.Refactor.Binding.Binder`) resolves every identifier in the syntax tree against the `DataContext` and produces a **bound query** (`BoundQuery`), a syntax-shaped tree where all names have been resolved to symbols and expressions have been typed.

## Entry point

```csharp
public static BindingResult Bind(SyntaxTree tree, DataContext dataContext);
```

## Binder hierarchy

The binder uses a chain-of-responsibility pattern with a recursive `Binder` base class and specialized subclasses:

| Binder                | Responsibility                                                                                                              |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `GlobalBinder`        | Root binder; introduces schema tables, functions, aggregates, variables, properties, and parameters from the `DataContext`. |
| `LocalBinder`         | Introduces locally scoped symbols (correlation columns, range variables).                                                   |
| `QueryBinder`         | Handles query-level name resolution (table references, column references in SELECT/GROUP BY/HAVING/ORDER BY).               |
| `WhereClauseBinder`   | Tracks that we are inside a WHERE clause (affects aggregate vs. non-aggregate resolution).                                  |
| `GroupByClauseBinder` | Tracks GROUP BY context.                                                                                                    |
| `JoinConditionBinder` | Tracks ON clause context with visibility of left and right table instances.                                                 |
| `OrderByClauseBinder` | Tracks ORDER BY context.                                                                                                    |

## Type Hierarchy

All bound nodes inherit from `BoundNode`:

```
BoundNode
├── BoundQuery
│   ├── BoundEmptyQuery
│   ├── BoundSelectQuery
│   ├── BoundOrderedQuery
│   ├── BoundUnionQuery
│   └── BoundIntersectOrExceptQuery
├── BoundCommonTableExpression
├── BoundSelectColumn
├── BoundWildcardSelectColumn
├── BoundOrderByColumn
├── BoundTableReference
│   ├── BoundNamedTableReference
│   ├── BoundDerivedTableReference
│   └── BoundJoinTableReference
└── BoundExpression
    ├── BoundLiteralExpression
    ├── BoundValueExpression
    ├── BoundVariableExpression
    ├── BoundColumnExpression
    ├── BoundPropertyAccessExpression
    ├── BoundTableExpression
    ├── BoundUnaryExpression
    ├── BoundBinaryExpression
    ├── BoundConversionExpression
    ├── BoundCaseExpression
    ├── BoundIsNullExpression
    ├── BoundAggregateExpression
    ├── BoundFunctionInvocationExpression
    ├── BoundMethodInvocationExpression
    ├── BoundExistsSubselect
    ├── BoundSingleRowSubselect
    └── BoundErrorExpression
```

## Values

The binder introduces **value identities** (`IBoundValue`) as the unit of data flow. Every column reference, computed value, and intermediate expression resolves to an `IBoundValue` — a single, typed value with stable reference identity. A column symbol *is* its own identity; anonymous values (computed expressions, aggregate outputs, set-op unified values, subquery results) are minted as `BoundValue` instances by the `BoundValueFactory`. The binder deals in identity only — it allocates no storage and assigns no slots.

The `ValueSlot` (and the indices into the runtime row buffer it ultimately becomes) is **not** a binder concept. It belongs to the algebra: the [algebrizer](algebrization.md) maps each `IBoundValue` to exactly one `ValueSlot`, minted on first use. See `IBoundValue` for the invariant that keeps this boundary clean (a binder identity must never carry a slot).

## Diagnostics

Binding produces semantic diagnostics (unknown tables, ambiguous columns, type mismatches, etc.) accessible via `BindingResult.Diagnostics`.
