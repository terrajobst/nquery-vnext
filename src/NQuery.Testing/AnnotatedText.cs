using System;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery
{
    public sealed partial class AnnotatedText
    {
        private readonly string _text;
        private readonly ImmutableArray<TextSpan> _spans;
        private readonly ImmutableArray<TextChange> _changes;

        public AnnotatedText(string text, ImmutableArray<TextSpan> spans, ImmutableArray<TextChange> changes)
        {
            _text = text;
            _spans = spans;
            _changes = changes;
        }

        public static AnnotatedText Parse(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var parser = new Parser(text);
            return parser.Parse();
        }

        public string Text
        {
            get { return _text; }
        }

        public ImmutableArray<TextSpan> Spans
        {
            get { return _spans; }
        }

        public ImmutableArray<TextChange> Changes
        {
            get { return _changes; }
        }
    }
}