using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.Outlining;

namespace NQuery.Authoring.Composition.Outlining
{
    [Export(typeof(IOutliningService))]
    internal sealed class OutliningService : IOutliningService
    {
        [ImportingConstructor]
        public OutliningService([ImportMany] IEnumerable<IOutliner> matchers)
        {
            Outliners = matchers.Concat(OutliningExtensions.GetStandardOutliners()).ToImmutableArray();
        }

        public ImmutableArray<IOutliner> Outliners { get; }
    }
}