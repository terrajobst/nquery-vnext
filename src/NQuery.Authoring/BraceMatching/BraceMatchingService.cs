using System.Collections.Immutable;

namespace NQuery.Authoring.BraceMatching;

public sealed class BraceMatchingService
{
    private readonly ImmutableArray<IBraceMatcher> _braceMatchers;

    public BraceMatchingService(ImmutableArray<IBraceMatcher> braceMatchers)
    {
        _braceMatchers = braceMatchers;
    }

    // First match wins, which is why the builder preserves registration order.
    public BraceMatchingResult MatchBraces(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var syntaxTree = view.Document.GetSyntaxTree(cancellationToken);

        return (from m in _braceMatchers
                let r = m.MatchBraces(syntaxTree, view.Position)
                where r.IsValid
                select r).DefaultIfEmpty(BraceMatchingResult.None).First();
    }
}
