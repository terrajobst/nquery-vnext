using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal abstract class BoundTableReference : BoundNode
    {
        public abstract IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances();
    }
}