using System.Collections.Immutable;

using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Completion;

public static class CompletionExtensions
{
    private static ImmutableArray<ICompletionProvider> StandardCompletionProviders { get; } =
    [
        new AliasCompletionProvider(),
        new JoinCompletionProvider(),
        new KeywordCompletionProvider(),
        new SymbolCompletionProvider(),
        new TypeCompletionProvider(),
        new CommonTableExpressionCompletionProvider()
    ];

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddCompletionService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new CompletionService(s.GetProviders<ICompletionProvider>()));
        }

        public AuthoringServicesBuilder AddCompletionProvider(ICompletionProvider provider)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<ICompletionProvider>(provider);
        }

        public AuthoringServicesBuilder AddStandardCompletionProviders()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardCompletionProviders);
        }
    }
}
