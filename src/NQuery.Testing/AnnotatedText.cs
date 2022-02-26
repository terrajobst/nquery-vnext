using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery
{
    public sealed partial class AnnotatedText
    {
        public AnnotatedText(string text, ImmutableArray<TextSpan> spans, ImmutableArray<TextChange> changes)
        {
            Text = text;
            Spans = spans;
            Changes = changes;
        }

        public static AnnotatedText Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var parser = new Parser(text);
            return parser.Parse();
        }

        public string Text { get; }

        public ImmutableArray<TextSpan> Spans { get; }

        public ImmutableArray<TextChange> Changes { get; }
    }
}