using System;
using System.Collections.Generic;

namespace NQuery.Authoring.Wpf
{
    internal abstract class ShowPlanItemViewModel
    {
        public abstract string DisplayName { get; }
        public abstract IEnumerable<ShowPlanItemViewModel> Children { get; }
        public abstract string Kind { get; }
    }
}