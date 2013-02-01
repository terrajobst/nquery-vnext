using System;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal abstract class BoundQuery : BoundNode
    {
        public abstract ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns { get; }
    }
}