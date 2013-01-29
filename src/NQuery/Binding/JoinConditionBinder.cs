using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class JoinConditionBinder : LocalBinder
    {
        public JoinConditionBinder(SharedBinderState sharedBinderState, Binder parent, IEnumerable<Symbol> localSymbols)
            : base(sharedBinderState, parent, localSymbols)
        {
        }

        protected override bool InOnClause
        {
            get { return true; }
        }
    }
}