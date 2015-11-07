using System;

using NQuery.Text;

namespace NQuery.Authoring.Outlining
{
    public struct OutliningRegionSpan
    {
        public OutliningRegionSpan(TextSpan span, string text)
        {
            Span = span;
            Text = text;
        }

        public TextSpan Span { get; }

        public string Text { get; }
    }
}