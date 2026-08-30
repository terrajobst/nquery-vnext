using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Highlighting;

public interface IHighlighter
{
    IEnumerable<TextSpan> GetHighlights(DocumentView view, CancellationToken cancellationToken);
}
