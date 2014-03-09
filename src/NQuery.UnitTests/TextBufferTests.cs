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
            var textBuffer = new TextBuffer(string.Empty);
            Assert.AreEqual(0, textBuffer.Text.Length);
            Assert.AreEqual(1, textBuffer.Lines.Count);
            Assert.AreEqual(new TextSpan(0, 0), textBuffer.Lines[0].TextSpan);
            Assert.AreEqual(new TextSpan(0, 0), textBuffer.Lines[0].TextSpanWithLineBreak);
        }

        [TestMethod]
        public void TextBuffer_SingleLineIsWithoutLineBreak()
        {
            const string text = "text";
            var textBuffer = new TextBuffer(text);

            Assert.AreEqual(text.Length, textBuffer.Text.Length);
            Assert.AreEqual(1, textBuffer.Lines.Count);

            Assert.AreEqual(new TextSpan(0, text.Length), textBuffer.Lines[0].TextSpan);
            Assert.AreEqual(new TextSpan(0, text.Length), textBuffer.Lines[0].TextSpanWithLineBreak);
            Assert.AreEqual(text, textBuffer.Lines[0].Text);
        }

        [TestMethod]
        public void TextBuffer_SingleWithLineBreakResultsInTwoLines()
        {
            const string first = "test";
            const string text = first + "\n";
            var textBuffer = new TextBuffer(text);

            Assert.AreEqual(text.Length, textBuffer.Text.Length);
            Assert.AreEqual(2, textBuffer.Lines.Count);

            Assert.AreEqual(new TextSpan(0, first.Length), textBuffer.Lines[0].TextSpan);
            Assert.AreEqual(new TextSpan(0, text.Length), textBuffer.Lines[0].TextSpanWithLineBreak);
            Assert.AreEqual(first, textBuffer.Lines[0].Text);

            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[1].TextSpan);
            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[1].TextSpanWithLineBreak);
            Assert.AreEqual(string.Empty, textBuffer.Lines[1].Text);
        }

        [TestMethod]
        public void TextBuffer_DifferentLineBreaksAreRecordedProperly()
        {
            const string first = "first";
            const string second = "second";
            const string third = "third";
            const string text = first + "\n" + second + "\r" + third + "\r\n";
            var textBuffer = new TextBuffer(text);

            Assert.AreEqual(text.Length, textBuffer.Text.Length);
            Assert.AreEqual(4, textBuffer.Lines.Count);

            Assert.AreEqual(new TextSpan(0, first.Length), textBuffer.Lines[0].TextSpan);
            Assert.AreEqual(new TextSpan(0, first.Length + 1), textBuffer.Lines[0].TextSpanWithLineBreak);
            Assert.AreEqual(first, textBuffer.Lines[0].Text);

            Assert.AreEqual(new TextSpan(first.Length + 1, second.Length), textBuffer.Lines[1].TextSpan);
            Assert.AreEqual(new TextSpan(first.Length + 1, second.Length + 1), textBuffer.Lines[1].TextSpanWithLineBreak);
            Assert.AreEqual(second, textBuffer.Lines[1].Text);

            Assert.AreEqual(new TextSpan(first.Length + second.Length + 2, third.Length), textBuffer.Lines[2].TextSpan);
            Assert.AreEqual(new TextSpan(first.Length + second.Length + 2, third.Length + 2), textBuffer.Lines[2].TextSpanWithLineBreak);
            Assert.AreEqual(third, textBuffer.Lines[2].Text);

            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[3].TextSpan);
            Assert.AreEqual(new TextSpan(text.Length, 0), textBuffer.Lines[3].TextSpanWithLineBreak);
            Assert.AreEqual(string.Empty, textBuffer.Lines[3].Text);
        }
    }
}