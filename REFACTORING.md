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
    - The physial nodes should have compiled expressions
* Add a translation layer Physical -> Iterator
* The `Optimizer` class handles both, optimization, as well as lowering. We
  should cleanly split this.

## Value Slots

* Simplify this entire thing

## Executable

* Make sure that all plans are executable

## Understanding

* Should symbols refer to value slots at all?
    - The binder deals with symbols. It seems we should leverage symbols somehow?
* What properties do we need to track?
    - Like sort order?
    - And where would those be tracked? On the logical operator?
* Is `ValueSlot` a good term? Or should we go with `ColumnId`?
* Should logical operators have explicit collections for defined value slots and returned value slots?
* How should outer references be modelled?