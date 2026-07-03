using System.Collections.Immutable;
using System.ComponentModel.Composition;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.Composition.BraceMatching;

[Export(typeof(IBraceMatcherService))]
internal sealed class BraceMatcherService : IBraceMatcherService
{
    [ImportingConstructor]
    public BraceMatcherService([ImportMany] IEnumerable<IBraceMatcher> matchers)
    {
        ThrowIfNull(matchers);

        Matchers = BraceMatchingExtensions.StandardBraceMatchers.AddRange(matchers);
    }

    public ImmutableArray<IBraceMatcher> Matchers { get; }
}
