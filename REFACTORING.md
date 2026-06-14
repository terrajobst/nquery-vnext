# NQuery Major Refactoring

## Completing the port

1. Common Table Expressions — bound but not executable (biggest gap).

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

2. No cost model / cardinality estimation.

CardinalityEstimator / CardinalityEstimate aren't ported. Planner is a
one-to-one lowering: hash-match build side is always the join's left, and
hash-vs-loops is structural (equi-key present), not cost-based.

## Other features

* Make LogicalConstant hold a static table and use it cases like this:
  ```SQL
  SELECT 1, 2, 3
  UNION ALL
  SELECT 4, 5, 6
  UNION ALL
  SELECT 7, 8, 9
  ```
* Add SQL's `VALUE()` constructor and use LogicalConstant
* Merge ComputeScalar() nodes
* Detect Filters/Join condition that are always false
* Add more correlation tests exploiting the new `CROSS APPLY` and `OUTER APPLY`
  to reach more cases

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

* Missing
  - Rename IBoundValue -> IBoundColumn and ValueSlot -> ColumnId. NOTE: "column" is
    already used by ColumnSymbol / ColumnInstanceSymbol / BoundColumnExpression, so
    IBoundColumn/BoundColumn will crowd that space; the slot rename is a large member
    churn (OutputValueSlots, DefinedValueSlots, LogicalValueSlotExpression, ...).
  - Instantiating CTEs
  - Add BoundCommonTableExpression that has AnchorMembers and RecursiveMembers
  - Should Empty/Constant just be a node that can return a table of literals?
  - Look at the legacy optimizer and compare it against the new pipeline. What
    optimizations are we performing already and which ones do we need to port?
  - Review naming of various constructs and check whether there a better
    industrial terms
  - Make sure that all plans are executable
  Representing AND and OR
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
* Fully split definitions and symbols
  - Make DataContext only deal with definitions
  - Simplify symbols
  - Replace ErrorTableSymbol with a BoundErrorTable
