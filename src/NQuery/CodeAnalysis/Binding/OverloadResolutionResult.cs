using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class OverloadResolutionResult<T>
    where T : Signature
{
    public OverloadResolutionResult(OverloadResolutionCandidate<T>? best, OverloadResolutionCandidate<T>? selected, IEnumerable<OverloadResolutionCandidate<T>> candidates)
    {
        ThrowIfNull(candidates);

        Best = best;
        Selected = selected;
        Candidates = [.. candidates];
    }

    public static OverloadResolutionResult<T> None { get; } = new(null, null, []);

    public OverloadResolutionCandidate<T>? Best { get; }

    public OverloadResolutionCandidate<T>? Selected { get; }

    public ImmutableArray<OverloadResolutionCandidate<T>> Candidates { get; }
}
