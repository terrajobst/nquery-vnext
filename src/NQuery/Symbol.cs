using System;

using NQuery.Symbols;

namespace NQuery
{
    // IsImplicitlyDeclared
    //   * Should return true for cases where we've created this symolb, e.g.
    //     SELECT FirstName FROM Employees (the query has an implicit definition for FirstName)
    //
    // DeclaringSyntaxNode
    //   * Should point to the node this was declared for.

    public abstract class Symbol
    {
        internal Symbol(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }

        public abstract SymbolKind Kind { get; }

        public string Name { get; }

        public abstract Type Type { get; }

        public sealed override string ToString()
        {
            return SymbolMarkup.ForSymbol(this).ToString();
        }
    }
}