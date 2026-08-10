using System.Collections.Immutable;

using NQuery.Authoring.Highlighting.Highlighters;

namespace NQuery.Authoring.Highlighting;

public static class HighlightingExtensions
{
    private static ImmutableArray<IHighlighter> StandardHighlighters { get; } =
    [
        new CaseKeywordHighlighter(),
        new CastKeywordHighlighter(),
        new SelectQueryKeywordHighlighter(),
        new OrderedQueryKeywordHighlighter(),
        new InnerJoinKeywordHighlighter(),
        new OuterJoinKeywordHighlighter(),
        new SymbolReferenceHighlighter()
    ];

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddHighlightingService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new HighlightingService(s.GetProviders<IHighlighter>()));
        }

        public AuthoringServicesBuilder AddHighlighter(IHighlighter highlighter)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<IHighlighter>(highlighter);
        }

        public AuthoringServicesBuilder AddStandardHighlighters()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardHighlighters);
        }
    }
}
