using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.Outlining;

namespace NQuery.Authoring.Composition.Outlining
{
    [Export(typeof(IOutliningService))]
    internal sealed class OutliningService : IOutliningService
    {
        private readonly ImmutableArray<IOutliner> _outliners;

        [ImportingConstructor]
        public OutliningService([ImportMany] IEnumerable<IOutliner> matchers)
        {
            _outliners = matchers.Concat(OutliningExtensions.GetStandardOutliners()).ToImmutableArray();
        }

        public ImmutableArray<IOutliner> Outliners
        {
            get { return _outliners; }
        }
    }
}