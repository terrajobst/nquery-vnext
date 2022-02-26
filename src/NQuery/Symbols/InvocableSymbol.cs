using System.Collections.Immutable;

namespace NQuery.Symbols
{
    public abstract class InvocableSymbol : Symbol
    {
        internal InvocableSymbol(string name, Type type, IEnumerable<ParameterSymbol> parameters)
            : base(name)
        {
            Type = type;
            Parameters = parameters.ToImmutableArray();
        }

        public override Type Type { get; }

        public ImmutableArray<ParameterSymbol> Parameters { get; }

        public IEnumerable<Type> GetParameterTypes()
        {
            return from p in Parameters
                   select p.Type;
        }
    }
}