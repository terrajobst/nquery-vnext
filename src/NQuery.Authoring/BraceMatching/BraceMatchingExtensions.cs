using System.Collections.Immutable;

using NQuery.Authoring.BraceMatching.Matchers;

namespace NQuery.Authoring.BraceMatching;

public static class BraceMatchingExtensions
{
    private static ImmutableArray<IBraceMatcher> StandardBraceMatchers { get; } =
    [
        new StringQuoteBraceMatcher(),
        new CaseBraceMatcher(),
        new DateBraceMatcher(),
        new IdentifierBraceMatcher(),
        new ParenthesisBraceMatcher(),
    ];

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddBraceMatchingService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new BraceMatchingService(s.GetProviders<IBraceMatcher>()));
        }

        public AuthoringServicesBuilder AddBraceMatcher(IBraceMatcher braceMatcher)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<IBraceMatcher>(braceMatcher);
        }

        public AuthoringServicesBuilder AddStandardBraceMatchers()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardBraceMatchers);
        }
    }
}
