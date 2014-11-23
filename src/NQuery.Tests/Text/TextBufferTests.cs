using System;

using NQuery.Text;

using Xunit;

namespace NQuery.Tests.Text
{
    public sealed class TextBufferTests
    {
        [Fact]
        public void TextBuffer_EmptyHasALineWithoutLineBreak()
        {
            var sourceText = SourceText.From(string.Empty);
            Assert.Equal(0, sourceText.Length);
            Assert.Equal(1, sourceText.Lines.Count);
            Assert.Equal(new TextSpan(0, 0), sourceText.Lines[0].Span);
            Assert.Equal(new TextSpan(0, 0), sourceText.Lines[0].SpanIncludingLineBreak);
        }

        [Fact]
        public void TextBuffer_SingleLineIsWithoutLineBreak()
        {
            const string text = "text";
            var sourceText = SourceText.From(text);

            Assert.Equal(text.Length, sourceText.Length);
            Assert.Equal(1, sourceText.Lines.Count);

            Assert.Equal(new TextSpan(0, text.Length), sourceText.Lines[0].Span);
            Assert.Equal(new TextSpan(0, text.Length), sourceText.Lines[0].SpanIncludingLineBreak);
            Assert.Equal(text, sourceText.Lines[0].GetText());
        }

        [Fact]
        public void TextBuffer_SingleWithLineBreakResultsInTwoLines()
        {
            const string first = "test";
            const string text = first + "\n";
            var sourceText = SourceText.From(text);

            Assert.Equal(text.Length, sourceText.Length);
            Assert.Equal(2, sourceText.Lines.Count);

            Assert.Equal(new TextSpan(0, first.Length), sourceText.Lines[0].Span);
            Assert.Equal(new TextSpan(0, text.Length), sourceText.Lines[0].SpanIncludingLineBreak);
            Assert.Equal(first, sourceText.Lines[0].GetText());

            Assert.Equal(new TextSpan(text.Length, 0), sourceText.Lines[1].Span);
            Assert.Equal(new TextSpan(text.Length, 0), sourceText.Lines[1].SpanIncludingLineBreak);
            Assert.Equal(string.Empty, sourceText.Lines[1].GetText());
        }

        [Fact]
        public void TextBuffer_DifferentLineBreaksAreRecordedProperly()
        {
            const string first = "first";
            const string second = "second";
            const string third = "third";
            const string text = first + "\n" + second + "\r" + third + "\r\n";
            var sourceText = SourceText.From(text);

            Assert.Equal(text.Length, sourceText.Length);
            Assert.Equal(4, sourceText.Lines.Count);

            Assert.Equal(new TextSpan(0, first.Length), sourceText.Lines[0].Span);
            Assert.Equal(new TextSpan(0, first.Length + 1), sourceText.Lines[0].SpanIncludingLineBreak);
            Assert.Equal(first, sourceText.Lines[0].GetText());

            Assert.Equal(new TextSpan(first.Length + 1, second.Length), sourceText.Lines[1].Span);
            Assert.Equal(new TextSpan(first.Length + 1, second.Length + 1), sourceText.Lines[1].SpanIncludingLineBreak);
            Assert.Equal(second, sourceText.Lines[1].GetText());

            Assert.Equal(new TextSpan(first.Length + second.Length + 2, third.Length), sourceText.Lines[2].Span);
            Assert.Equal(new TextSpan(first.Length + second.Length + 2, third.Length + 2), sourceText.Lines[2].SpanIncludingLineBreak);
            Assert.Equal(third, sourceText.Lines[2].GetText());

            Assert.Equal(new TextSpan(text.Length, 0), sourceText.Lines[3].Span);
            Assert.Equal(new TextSpan(text.Length, 0), sourceText.Lines[3].SpanIncludingLineBreak);
            Assert.Equal(string.Empty, sourceText.Lines[3].GetText());
        }
    }
}