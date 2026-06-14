using System.Collections.Immutable;

namespace NQuery.Binding
{
    // Mints BoundValue identities for anonymous values during binding. Unlike the algebra's
    // ValueSlotFactory it allocates no storage and assigns no slots -- it only hands out
    // identities with readable, deduplicated names. The algebrizer assigns the actual ValueSlots.
    internal sealed class BoundValueFactory
    {
        private const string TemporaryFormatString = @"Expr{0}";
        private ImmutableDictionary<string, int> _usedNames = ImmutableDictionary<string, int>.Empty;

        public BoundValue Create(string formatString, Type type)
        {
            var number = ImmutableInterlocked.AddOrUpdate(ref _usedNames, formatString, 1, (_, v) => v + 1);
            return new BoundValue(string.Format(formatString, number), type);
        }

        public BoundValue CreateTemporary(Type type)
        {
            return Create(TemporaryFormatString, type);
        }

        public BoundValue CreateNamed(string name, Type type)
        {
            var formatString = $"{name}:{{0}}";
            return Create(formatString, type);
        }
    }
}
