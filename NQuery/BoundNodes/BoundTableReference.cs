using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal abstract class BoundTableReference : BoundNode
    {
        public abstract IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances();
    }
}