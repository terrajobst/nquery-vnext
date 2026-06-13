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

## Value slots

The binder introduces **value slots** (`ValueSlot`) as the unit of data flow. Every column reference, computed value, and intermediate expression is assigned a slot. The `ValueSlotFactory` owns slot creation, and the `BoundValueFactory` maps `IBoundValue` instances to their `ValueSlot`.

Value slots flow through the entire pipeline: they appear in the bound tree, the logical algebra, the physical plan, the executable plan, and finally as indices into the runtime row buffer.

## Diagnostics

Binding produces semantic diagnostics (unknown tables, ambiguous columns, type mismatches, etc.) accessible via `BindingResult.Diagnostics`.
