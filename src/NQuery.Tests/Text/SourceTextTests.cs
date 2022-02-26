using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Tests.Text
{
    public sealed class SourceTextTests
    {
        [Fact]
        public void SourceText_FromStringHasStaticContainer()
        {
            var sourceText = SourceText.From("String");
            Assert.IsType<StaticSourceTextContainer>(sourceText.Container);
        }

        [Fact]
        public void SourceText_EmptyHasALineWithoutLineBreak()
        {
            var sourceText = SourceText.From(string.Empty);
            Assert.Equal(0, sourceText.Length);
            Assert.Single(sourceText.Lines);
            Assert.Equal(new TextSpan(0, 0), sourceText.Lines[0].Span);
            Assert.Equal(new TextSpan(0, 0), sourceText.Lines[0].SpanIncludingLineBreak);
        }

        [Fact]
        public void SourceText_SingleLineIsWithoutLineBreak()
        {
            const string text = "text";
            var sourceText = SourceText.From(text);

            Assert.Equal(text.Length, sourceText.Length);
            Assert.Single(sourceText.Lines);

            Assert.Equal(new TextSpan(0, text.Length), sourceText.Lines[0].Span);
            Assert.Equal(new TextSpan(0, text.Length), sourceText.Lines[0].SpanIncludingLineBreak);
            Assert.Equal(text, sourceText.Lines[0].GetText());
        }

        [Fact]
        public void SourceText_SingleWithLineBreakResultsInTwoLines()
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
        public void SourceText_DifferentLineBreaksAreRecordedProperly()
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

        [Fact]
        public void SourceText_ReturnsAllChanges()
        {
            var original = SourceText.From("ABCDEFGH");
            var modified1 = original.Replace(1, 2, "XY");
            var modified2 = modified1.Replace(7, 1, "Z");

            var allChanges = modified2.GetChanges(original).ToImmutableArray();
            Assert.Equal(2, allChanges.Length);
            Assert.Equal(1, allChanges[0].Span.Start);
            Assert.Equal(2, allChanges[0].Span.Length);
            Assert.Equal("XY", allChanges[0].NewText);
            Assert.Equal(7, allChanges[1].Span.Start);
            Assert.Equal(1, allChanges[1].Span.Length);
            Assert.Equal("Z", allChanges[1].NewText);
        }

        [Fact]
        public void SourceText_ReturnsLastChange()
        {
            var original = SourceText.From("ABCDEFGH");
            var modified1 = original.Replace(1, 2, "XY");
            var modified2 = modified1.Replace(7, 1, "Z");

            var lastChanges = modified2.GetChanges(modified1).ToImmutableArray();
            Assert.Single(lastChanges);
            Assert.Equal(7, lastChanges[0].Span.Start);
            Assert.Equal(1, lastChanges[0].Span.Length);
            Assert.Equal("Z", lastChanges[0].NewText);
        }

        [Fact]
        public void SourceText_ReturnsNoChanges_ForSelf()
        {
            var original = SourceText.From("ABCDEFGH");
            var modified1 = original.Replace(1, 2, "XY");
            var modified2 = modified1.Replace(7, 1, "Z");

            var noChanges = modified2.GetChanges(modified2).ToImmutableArray();
            Assert.Empty(noChanges);
        }

        [Fact]
        public void SourceText_ReturnsChanges_ForDifferentSourceTexts()
        {
            var sourceText1 = SourceText.From("ABCDEFGH");
            var sourceText2 = SourceText.From("XYZ");

            var allChanges = sourceText2.GetChanges(sourceText1).ToImmutableArray();
            Assert.Single(allChanges);
            Assert.Equal(0, allChanges[0].Span.Start);
            Assert.Equal(sourceText1.Length, allChanges[0].Span.Length);
            Assert.Equal(sourceText2.GetText(), allChanges[0].NewText);
        }

        [Fact]
        public void SourceText_WithChanges_AppliedInReverseOrder()
        {
            var sourceText = SourceText.From("ABCDEFGH");
            var changes = new[]
            {
                new TextChange(new TextSpan(0, 4), ""),
                new TextChange(new TextSpan(6, 2), "xy"),
            };

            var resultSourceText = sourceText.WithChanges(changes).GetText();
            Assert.Equal("EFxy", resultSourceText);
        }

        [Fact]
        public void SourceText_WithChanges_DetectsOverlappingChanges()
        {
            var sourceText = SourceText.From("acbdEFGH");

            var changes = new[]
            {
                new TextChange(new TextSpan(3, 2), "XYZ"),
                new TextChange(new TextSpan(4, 4), ""),
            };

            var exception = Assert.Throws<InvalidOperationException>(() => sourceText.WithChanges(changes));
            Assert.Equal("Changes must not overlap.", exception.Message);
        }

        [Fact]
        public void SourceText_WithChanges_ReturnsSelfIfNoChanges()
        {
            var sourceText = SourceText.From("ABCDEFGH");
            var changes = new[]
            {
                new TextChange(new TextSpan(1, 3), "BCD"),
                new TextChange(new TextSpan(6, 2), "GH"),
            };

            var resultSourceText = sourceText.WithChanges(changes);

            Assert.Same(sourceText, resultSourceText);
        }
    }
}