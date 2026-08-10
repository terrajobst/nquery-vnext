using System.Collections.Immutable;

using NQuery.Authoring.Selection.Providers;

namespace NQuery.Authoring.Selection;

public static class SelectionExtensions
{
    private static ImmutableArray<ISelectionSpanProvider> StandardSelectionSpanProviders { get; } =
    [
        new ArgumentListSelectionSpanProvider(),
        new CommonTableExpressionColumnNameListSelectionSpanProvider(),
        new CommonTableExpressionQuerySelectionSpanProvider(),
        new FromClauseSelectionSpanProvider(),
        new GroupByClauseSelectionSpanProvider(),
        new OrderedQuerySelectionSpanProvider(),
        new SelectClauseSelectionSpanProvider()
    ];

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddSelectionService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new SelectionService(s.GetProviders<ISelectionSpanProvider>()));
        }

        public AuthoringServicesBuilder AddSelectionSpanProvider(ISelectionSpanProvider provider)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<ISelectionSpanProvider>(provider);
        }

        public AuthoringServicesBuilder AddStandardSelectionSpanProviders()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardSelectionSpanProviders);
        }
    }
}
