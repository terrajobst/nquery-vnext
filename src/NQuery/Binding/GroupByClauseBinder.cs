using System;

namespace NQuery.Binding
{
    internal sealed class GroupByClauseBinder : Binder
    {
        public GroupByClauseBinder(SharedBinderState sharedBinderState, Binder parent)
            : base(sharedBinderState, parent)
        {
        }

        protected override bool InGroupByClause
        {
            get { return true; }
        }
    }
}