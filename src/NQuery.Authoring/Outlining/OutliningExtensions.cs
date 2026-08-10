using System.Collections.Immutable;

using NQuery.Authoring.Outlining.Outliners;

namespace NQuery.Authoring.Outlining;

public static class OutliningExtensions
{
    private static ImmutableArray<IOutliner> StandardOutliners { get; } =
    [
        new SelectQueryOutliner(),
        new OrderedQueryOutliner(),
        new MultiLineCommentOutliner(),
        new SingleLineCommentOutliner()
    ];

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddOutliningService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new OutliningService(s.GetProviders<IOutliner>()));
        }

        public AuthoringServicesBuilder AddOutliner(IOutliner outliner)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<IOutliner>(outliner);
        }

        public AuthoringServicesBuilder AddStandardOutliners()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardOutliners);
        }
    }
}
