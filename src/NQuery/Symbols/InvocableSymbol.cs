using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Symbols
{
    public abstract class InvocableSymbol : Symbol
    {
        private readonly Type _type;
        private readonly ImmutableArray<ParameterSymbol> _parameters;

        protected InvocableSymbol(string name, Type type, IEnumerable<ParameterSymbol> parameters)
            : base(name)
        {
            _type = type;
            _parameters = parameters.ToImmutableArray();
        }

        public override Type Type
        {
            get { return _type; }
        }

        public ImmutableArray<ParameterSymbol> Parameters
        {
            get { return _parameters; }
        }

        public IEnumerable<Type> GetParameterTypes()
        {
            return from p in Parameters
                   select p.Type;
        }
    }
}