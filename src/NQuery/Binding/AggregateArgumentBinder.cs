using System;

namespace NQuery.Binding
{
    internal sealed class AggregateArgumentBinder : Binder
    {
        public AggregateArgumentBinder(SharedBinderState sharedBinderState, Binder parent)
            : base(sharedBinderState, parent)
        {
        }

        protected override bool InAggregateArgument
        {
            get { return true; }
        }
    }
}