using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal abstract class BoundTableReference : BoundNode
    {
        public abstract IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances();
    }
}