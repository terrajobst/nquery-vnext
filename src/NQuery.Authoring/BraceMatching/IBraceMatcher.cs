namespace NQuery.Authoring.BraceMatching;

public interface IBraceMatcher
{
    BraceMatchingResult MatchBraces(DocumentView view, CancellationToken cancellationToken);
}
