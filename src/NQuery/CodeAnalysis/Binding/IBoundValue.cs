namespace NQuery.CodeAnalysis.Binding;

// The identity of a value produced by the binder: a single, typed value with stable
// reference identity. It answers the binder's question -- "are these two references the
// same value as written in the query?" -- and nothing more. The algebrizer later maps each
// IBoundValue to a ValueSlot, which answers the algebra's question: "which row-buffer column
// carries it?".
//
// INVARIANT: this exposes identity (reference) + Type ONLY. It must never carry a ValueSlot --
// slot assignment belongs to the algebrizer, and welding a slot back onto this would
// re-create the very coupling this interface exists to remove.
//
// Implemented by column symbols (a column is its own value identity) and by BoundValue
// (anonymous computed/aggregate/set-op/subquery values). NOT implemented by variables or
// literals -- those never occupy a slot.
internal interface IBoundValue
{
    Type Type { get; }
}
