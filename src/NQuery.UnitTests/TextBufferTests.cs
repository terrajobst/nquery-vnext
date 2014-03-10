using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Text;

namespace NQuery.UnitTests
{
    [TestClass]
    public sealed class TextBufferTests
    {
        [TestMethod]
        public void TextBuffer_EmptyHasALineWithoutLineBreak()
        {
            var textBuffer = TextBuffer.From(string.Empty);
            Assert.AreEqual(0, textBuffer.Length);
            Assert.AreEqual(1, textBuffer.Lines.Count);
            Assert.AreEqual(new TextSpan(0, 0), textBuffer.Lines[0].Span);
            Assert.AreEqual(new TextSpan(0, 0), textBuffer.Lines[0].SpanWithLineBreak);
        }

        [TestMethod]
        public void TextBuffer_SingleLineIsWithoutLineBreak()
        {
            const string text = "text";
            var textBuffer = TextBuffer.From(text);

            Assert.AreEqual(text.Length, textBuffer.Length);
            Assert.AreEqual(1, textBuffer.Lines.Count);

            Assert.AreEqual(new TextSpan(0, text.Length), textBuffer.Lines[0].Span);
            Assert.AreEqual(new TextSpan(0, text.Length), textBuffer.Lines[0].SpanWithLineBreak);
            Assert.AreEqual(text, textBuffer.Lines[0].GetText());
        }

        [TestMethod]
        public void TextBuffer_SingleWithLineBreakResultsInTwoLines()
        {
            const string first = "test";
            const string text = first + "\n";
            var textBuffer = TextBuffer.From(text);

            Assert.AreEqual(text.Length, textBuffer.Length);
            Assert.AreEqual(2, textBuffer.Lines.Count);

            Assert.AreEqual(new TextSpan(0, first.Length), textBuffer.Lines[0].Span);
            Assert.AreEqual(new TextSpan(0, text.Length), textBuffer.Lines[0].SpanWithLineBreak);
            Assert.AreEqual(first, textBuffer.Lines[0].GetText());

            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[1].Span);
            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[1].SpanWithLineBreak);
            Assert.AreEqual(string.Empty, textBuffer.Lines[1].GetText());
        }

        [TestMethod]
        public void TextBuffer_DifferentLineBreaksAreRecordedProperly()
        {
            const string first = "first";
            const string second = "second";
            const string third = "third";
            const string text = first + "\n" + second + "\r" + third + "\r\n";
            var textBuffer = TextBuffer.From(text);

            Assert.AreEqual(text.Length, textBuffer.Length);
            Assert.AreEqual(4, textBuffer.Lines.Count);

            Assert.AreEqual(new TextSpan(0, first.Length), textBuffer.Lines[0].Span);
            Assert.AreEqual(new TextSpan(0, first.Length + 1), textBuffer.Lines[0].SpanWithLineBreak);
            Assert.AreEqual(first, textBuffer.Lines[0].GetText());

            Assert.AreEqual(new TextSpan(first.Length + 1, second.Length), textBuffer.Lines[1].Span);
            Assert.AreEqual(new TextSpan(first.Length + 1, second.Length + 1), textBuffer.Lines[1].SpanWithLineBreak);
            Assert.AreEqual(second, textBuffer.Lines[1].GetText());

            Assert.AreEqual(new TextSpan(first.Length + second.Length + 2, third.Length), textBuffer.Lines[2].Span);
            Assert.AreEqual(new TextSpan(first.Length + second.Length + 2, third.Length + 2), textBuffer.Lines[2].SpanWithLineBreak);
            Assert.AreEqual(third, textBuffer.Lines[2].GetText());

            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[3].Span);
            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[3].SpanWithLineBreak);
            Assert.AreEqual(string.Empty, textBuffer.Lines[3].GetText());
        }
    }
}