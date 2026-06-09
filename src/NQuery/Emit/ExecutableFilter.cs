#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    internal sealed class ExecutableFilter : ExecutableOperator
    {
        private readonly ExecutableOperator _input;
        private readonly EmittedPredicate _predicate;

        public ExecutableFilter(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input, ImmutableArray<LogicalExpression> conditions)
            : base(outputValueSlots)
        {
            _input = input;

            // Compile the conjuncts once, against the input's slot layout. The
            // predicate takes the row buffer at run time, so it is reusable.
            var slotIndices = EmittedExpressionCompiler.CreateSlotIndices(input.OutputValueSlots);
            var predicates = conditions
                             .Select(c => EmittedExpressionCompiler.CompilePredicate(c, slotIndices))
                             .ToImmutableArray();
            _predicate = Conjoin(predicates);
        }

        public override Iterator CreateIterator()
        {
            return new EmittedFilterIterator(_input.CreateIterator(), _predicate);
        }

        // Each conjunct already yields false on NULL, so AND-ing gives WHERE semantics.
        private static EmittedPredicate Conjoin(ImmutableArray<EmittedPredicate> predicates)
        {
            if (predicates.Length == 1)
                return predicates[0];

            return rowBuffer =>
            {
                foreach (var predicate in predicates)
                {
                    if (!predicate(rowBuffer))
                        return false;
                }

                return true;
            };
        }
    }
}
