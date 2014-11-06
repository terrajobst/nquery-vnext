using System;

using NQuery.Text;

using Xunit;

namespace NQuery.UnitTests.Text
{
    public sealed class TextBufferTests
    {
        [Fact]
        public void TextBuffer_EmptyHasALineWithoutLineBreak()
        {
            var textBuffer = TextBuffer.From(string.Empty);
            Assert.Equal(0, textBuffer.Length);
            Assert.Equal(1, textBuffer.Lines.Count);
            Assert.Equal(new TextSpan(0, 0), textBuffer.Lines[0].Span);
            Assert.Equal(new TextSpan(0, 0), textBuffer.Lines[0].SpanWithLineBreak);
        }

        [Fact]
        public void TextBuffer_SingleLineIsWithoutLineBreak()
        {
            const string text = "text";
            var textBuffer = TextBuffer.From(text);

            Assert.Equal(text.Length, textBuffer.Length);
            Assert.Equal(1, textBuffer.Lines.Count);

            Assert.Equal(new TextSpan(0, text.Length), textBuffer.Lines[0].Span);
            Assert.Equal(new TextSpan(0, text.Length), textBuffer.Lines[0].SpanWithLineBreak);
            Assert.Equal(text, textBuffer.Lines[0].GetText());
        }

        [Fact]
        public void TextBuffer_SingleWithLineBreakResultsInTwoLines()
        {
            const string first = "test";
            const string text = first + "\n";
            var textBuffer = TextBuffer.From(text);

            Assert.Equal(text.Length, textBuffer.Length);
            Assert.Equal(2, textBuffer.Lines.Count);

            Assert.Equal(new TextSpan(0, first.Length), textBuffer.Lines[0].Span);
            Assert.Equal(new TextSpan(0, text.Length), textBuffer.Lines[0].SpanWithLineBreak);
            Assert.Equal(first, textBuffer.Lines[0].GetText());

            Assert.Equal(new TextSpan(text.Length, 0), textBuffer.Lines[1].Span);
            Assert.Equal(new TextSpan(text.Length, 0), textBuffer.Lines[1].SpanWithLineBreak);
            Assert.Equal(string.Empty, textBuffer.Lines[1].GetText());
        }

        [Fact]
        public void TextBuffer_DifferentLineBreaksAreRecordedProperly()
        {
            const string first = "first";
            const string second = "second";
            const string third = "third";
            const string text = first + "\n" + second + "\r" + third + "\r\n";
            var textBuffer = TextBuffer.From(text);

            Assert.Equal(text.Length, textBuffer.Length);
            Assert.Equal(4, textBuffer.Lines.Count);

            Assert.Equal(new TextSpan(0, first.Length), textBuffer.Lines[0].Span);
            Assert.Equal(new TextSpan(0, first.Length + 1), textBuffer.Lines[0].SpanWithLineBreak);
            Assert.Equal(first, textBuffer.Lines[0].GetText());

            Assert.Equal(new TextSpan(first.Length + 1, second.Length), textBuffer.Lines[1].Span);
            Assert.Equal(new TextSpan(first.Length + 1, second.Length + 1), textBuffer.Lines[1].SpanWithLineBreak);
            Assert.Equal(second, textBuffer.Lines[1].GetText());

            Assert.Equal(new TextSpan(first.Length + second.Length + 2, third.Length), textBuffer.Lines[2].Span);
            Assert.Equal(new TextSpan(first.Length + second.Length + 2, third.Length + 2), textBuffer.Lines[2].SpanWithLineBreak);
            Assert.Equal(third, textBuffer.Lines[2].GetText());

            Assert.Equal(new TextSpan(text.Length, 0), textBuffer.Lines[3].Span);
            Assert.Equal(new TextSpan(text.Length, 0), textBuffer.Lines[3].SpanWithLineBreak);
            Assert.Equal(string.Empty, textBuffer.Lines[3].GetText());
        }
    }
}