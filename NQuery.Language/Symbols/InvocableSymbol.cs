using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Language.Symbols
{
    public abstract class InvocableSymbol : Symbol
    {
        private readonly Type _type;
        private readonly ReadOnlyCollection<ParameterSymbol> _parameters;

        protected InvocableSymbol(string name, Type type, IList<ParameterSymbol> parameters)
            : base(name)
        {
            _type = type;
            _parameters = new ReadOnlyCollection<ParameterSymbol>(parameters);
        }

        public override Type Type
        {
            get { return _type; }
        }

        public ReadOnlyCollection<ParameterSymbol> Parameters
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