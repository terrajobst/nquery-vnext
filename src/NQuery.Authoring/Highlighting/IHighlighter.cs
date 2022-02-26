using NQuery.Text;

namespace NQuery.Authoring.Highlighting
{
    public interface IHighlighter
    {
        IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, int position);
    }
}