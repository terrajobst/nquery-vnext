using NQuery.Text;

namespace NQuery.Authoring.Selection
{
    public interface ISelectionSpanProvider
    {
        IEnumerable<TextSpan> Provide(SyntaxNodeOrToken nodeOrToken);
    }
}