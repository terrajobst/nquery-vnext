using System;

namespace NQuery.Language.Symbols
{
    public abstract class Symbol
    {
        private readonly string _name;

        internal Symbol(string name)
        {
            _name = name;
        }

        public abstract SymbolKind Kind { get; }

        public string Name
        {
            get { return _name; }
        }

        public abstract Type Type { get; }
    }
}