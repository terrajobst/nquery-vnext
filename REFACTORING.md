# NQuery Major Refactoring

* Are properties on the show plan used at all?
* Do we need the step functions in BuiltInAggregates? Seems like should emit
  them as expressions.
* What does the emitter have an abstraction (Aggregator and FoldAggregator)?
* Can we change the for loops for all aggregates to more or less be emitted as well?
  - It seems we need this if we want the aggregates to be stored in strongly
    typed slots eventually. Doing so probably means that Aggregator can
    disappear entirely and instead we have three void returning functions that
    we compile during emit (initialize, accumulate, get result)
* Use Int128 on .NET Core, for .NET Framework rename the Bit128 type to
  System.Int128, but the shape identical. Now need to copy .NET Core's type.
  Can we make the fields private?
* I think each part we iterator over a group of this that read or write from a
  row buffer (table scan) we should have a single Action that was compiled with
  the loop unrolled.
* We should make we can run the tests on .NET Framework and on .NET Core.

---

* Do we have any public fields? They should be properties.
* Do we have static GetXxx() or properties returning `IEnumerable<T>` that
  should really be materialized `ImmutableArray<T>`?
* I don't like that the algebrizer needs to create symbols and binds Count and
  Any.
* When creating tables, columns, properties, and methods we need to normalize
  int? to int. During overload resolution we should always prefer the nullable
  one. Or do we need that? I'd assume we'd prefer the non-nullable one because
  of the way we handle conversions (our types are always non-null T and T is
  always better than an implicit conversion from T to T?)
* Fix the indentation of new Catalog()

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

## More test coverage

* Make sure that all plans are executable
* Add more correlation tests exploiting the new `CROSS APPLY` and `OUTER APPLY`
  to reach more cases
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
* Use new C# language features
* Change the authoring to have a root-object that we can add language services
  to via extension methods. Maybe a WorkspaceBuilder?
* CompiledQuery and ExpressionEvaluator should go to CodeAnalysis
* ExpressionEvaluator should probably be renamed to CompiledExpression.
* Ideally we'd have `CompiledExpression<T>`
* We still need to support cases where the type isn't known until runtime. That
  should probably be `compilation.CompileExpression(typeof(SomeType))` that
  returns `Expression<object>`.
* Probably means `CompiledExpression<T>` should have a target type.
* Are properties on the show plan used at all?
