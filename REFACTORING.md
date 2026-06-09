# NQuery Major Refactoring

## Increase test coverage:

* Binder
* Optimizer
* Lowerer
* Compiler

### Additions

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

## Physical Operator Layer

* Add new tree hierarchy for Physical operators
* Add a translation layer Logical -> Physical
    - The physical nodes should have compiled expressions
* Add a translation layer Physical -> Iterator
* The `Optimizer` class handles both, optimization, as well as lowering. We
  should cleanly split this.
* Namespace: `NQuery.Planning`
* BaseNode: `PhysicalOperator`What

## Value Slots

* Simplify this entire thing

## Executable

* Make sure that all plans are executable

## Understanding

* Should symbols refer to value slots at all?
    - No, we should remove that
* What properties do we need to track?
    - Like sort order?
    - And where would those be tracked? On the logical operator?
* Is `ValueSlot` a good term? Or should we go with `ColumnId`?
* Should logical operators have explicit collections for defined value slots and returned value slots?
* How should outer references be modelled?
    - An explicit ApplyNode
* Representing AND and OR
    - Use N-ary AND and OR
    - Use NNF
    - Track conjunction lists at filter and join

## Components

* Lexer.Lex() -> SyntaxToken
* Parser.Parse() -> SyntaxNode
* Binder.Bind() -> BoundNode
* Algebrizer.Algebrize() -> LogicalOperator
* Optimizer.Optimize() -> LogicalOperator
* Planner.Plan() -> PhysicalOperator
* Emitter.Emit() -> ExecutablePlan
* ExecutablePlan.CreateIterator() -> Iterator

We should split today's `Iterator` into `ExecutablePlan` and `Iterator`:

* Emitter.Emit(PhysicalOperator) -> ExecutablePlan — the reusable artifact: plan
  structure + the compiled delegates (IteratorFunction/IteratorPredicate) +
  row-buffer layout. Built once, cached on CompiledQuery. Because the only thing
  that's genuinely "emitted code" in your engine is those compiled delegates,
  and they are reusable — so emitting them once is the real codegen step, and
  the name now matches the "reusable like IL" intuition.
  
* ExecutablePlan.Open() -> Cursor (or CreateCursor) — the per-execution step
  that allocates the RowBuffers and cursor position. Cheap, allocates only
  mutable state, runnable many times and concurrently. This is your
  ExecInitNode/Volcano-Open. It is not a pipeline phase — it's runtime
  instantiation, deliberately outside the seven-phase list.

Primary recommendation — Executable… prefix, base ExecutableOperator:

  ExecutablePlan            // the reusable tree (Emitter output, what you cache)
  ExecutableOperator        // node base
    ExecutableFilter, ExecutableSort, ExecutableHashMatch, ExecutableTableScan, 