using System;
using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Symbol Symbol { get; }
        public abstract IEnumerable<Symbol> Candidates { get; }
        public abstract Type Type { get; }
    }
}