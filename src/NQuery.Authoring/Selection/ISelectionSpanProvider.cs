using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Selection;

public interface ISelectionSpanProvider
{
    // The spans this provider offers around the view's selection, innermost first. The service
    // picks between providers by size, so a provider that returns them out of order still works --
    // but a nesting chain is what the shape is meant to express.
    IEnumerable<TextSpan> GetSpans(DocumentView view, CancellationToken cancellationToken);
}
