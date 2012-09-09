using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal abstract class BoundExpression : BoundNode
    {
        // TODO: Do we really need to this property? It seems we should special case the actually bound expressions.
        public virtual Symbol Symbol
        {
            get { return null; }
        }

        // TODO: Do we really need to this property? It seems we should special case the actually bound expressions.
        public virtual IEnumerable<Symbol> Candidates
        {
            get { return Enumerable.Empty<Symbol>();}
        }

        public abstract Type Type { get; }
    }
}