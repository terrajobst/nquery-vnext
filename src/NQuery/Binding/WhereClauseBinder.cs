using System;

namespace NQuery.Binding
{
    internal sealed class WhereClauseBinder : Binder
    {
        public WhereClauseBinder(SharedBinderState sharedBinderState, Binder parent)
            : base(sharedBinderState, parent)
        {
        }

        protected override bool InWhereClause
        {
            get { return true; }
        }
    }
}