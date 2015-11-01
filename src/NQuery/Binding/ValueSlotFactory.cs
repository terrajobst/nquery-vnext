using System;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class ValueSlotFactory
    {
        private const string TemporaryFormatString = @"Expr{0}";
        private ImmutableDictionary<string, int> _usedNames = ImmutableDictionary<string, int>.Empty;

        public ValueSlotFactory()
        {
            _usedNames.Add(TemporaryFormatString, 1000);
        }

        public ValueSlot Create(string formatString, Type type)
        {
            var number = ImmutableInterlocked.AddOrUpdate(ref _usedNames, formatString, 1, (k, v) => v + 1);
            return new ValueSlot(this, formatString, number, type);
        }

        public ValueSlot CreateTemporary(Type type)
        {
            return Create(TemporaryFormatString, type);
        }

        public ValueSlot CreateNamed(string name, Type type)
        {
            var formatString = $"{name}:{{0}}";
            return Create(formatString, type);
        }
    }
}