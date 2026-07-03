using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Outlining;

public readonly struct OutliningRegionSpan
{
    public OutliningRegionSpan(TextSpan span, string text)
    {
        ThrowIfNull(text);

        Span = span;
        Text = text;
    }

    public TextSpan Span { get; }

    public string Text { get; }
}
