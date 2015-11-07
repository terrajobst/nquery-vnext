using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Symbols
{
    public abstract class InvocableSymbol : Symbol
    {
        protected InvocableSymbol(string name, Type type, IEnumerable<ParameterSymbol> parameters)
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