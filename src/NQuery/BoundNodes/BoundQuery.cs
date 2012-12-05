using System;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal abstract class BoundQuery : BoundNode
    {
        public abstract ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns { get; }
    }
}