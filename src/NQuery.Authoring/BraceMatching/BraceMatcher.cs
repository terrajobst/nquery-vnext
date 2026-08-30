using NQuery.CodeAnalysis;

namespace NQuery.Authoring.BraceMatching;

public abstract class BraceMatcher : IBraceMatcher
{
    public BraceMatchingResult MatchBraces(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var syntaxTree = view.Document.GetSyntaxTree(cancellationToken);
        var position = view.Position;

        return syntaxTree.Root.FindStartTokens(position)
                              .Select(t => MatchBraces(t, position))
                              .Where(r => r.IsValid)
                              .DefaultIfEmpty(BraceMatchingResult.None)
                              .First();
    }

    protected abstract BraceMatchingResult MatchBraces(SyntaxToken syntaxTree, int position);
}
