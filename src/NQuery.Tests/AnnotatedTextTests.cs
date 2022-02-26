using NQuery.Text;

using Xunit;

namespace NQuery.Tests
{
    public class AnnotatedTextTests
    {
        [Fact]
        public static void AnnotatedText_ParsesPositions()
        {
            var text = "Lorem | ip|sum";
            var annotatedText = AnnotatedText.Parse(text);

            var expectedText = "Lorem  ipsum";
            var expectedPositions = new[]
            {
                new TextSpan(6, 0),
                new TextSpan(9, 0)
            };

            Assert.Equal(expectedText, annotatedText.Text);
            Assert.Equal(expectedPositions, annotatedText.Spans);
            Assert.Empty(annotatedText.Changes);
        }

        [Fact]
        public static void AnnotatedText_ParsesSpans()
        {
            var text = "L{or}em{} ip{sum}";
            var annotatedText = AnnotatedText.Parse(text);

            var expectedText = "Lorem ipsum";
            var expectedSpans = new[]
            {
                new TextSpan(1, 2),
                new TextSpan(5, 0),
                new TextSpan(8, 3)
            };

            Assert.Equal(expectedText, annotatedText.Text);
            Assert.Equal(expectedSpans, annotatedText.Spans);
            Assert.Empty(annotatedText.Changes);
        }

        [Fact]
        public static void AnnotatedText_ParsesSpans_WhenNested()
        {
            var text = "Root { span { with a nested } }";
            var annotatedText = AnnotatedText.Parse(text);

            var expectedText = "Root  span  with a nested  ";
            var expectedSpans = new[]
            {
                new TextSpan(5, 22),
                new TextSpan(11, 15)
            };

            Assert.Equal(expectedText, annotatedText.Text);
            Assert.Equal(expectedSpans, annotatedText.Spans);
            Assert.Empty(annotatedText.Changes);
        }

        [Fact]
        public static void AnnotatedText_ParsesChanges()
        {
            var text = "L{or:a}em{: test} ip{sum:}";
            var annotatedText = AnnotatedText.Parse(text);

            var expectedText = "Lorem ipsum";
            var expectedChanges = new[]
            {
                new TextChange(new TextSpan(1, 2), "a"),
                new TextChange(new TextSpan(5, 0), " test"),
                new TextChange(new TextSpan(8, 3), string.Empty)
            };

            Assert.Equal(expectedText, annotatedText.Text);
            Assert.Empty(annotatedText.Spans);
            Assert.Equal(expectedChanges, annotatedText.Changes);
        }

        [Fact]
        public static void AnnotatedText_ParsesCombination()
        {
            var text = @"
                This| {is} a {replacement:change}. At| this {:brand new }{span}.
            ".NormalizeCode();

            var annotatedText = AnnotatedText.Parse(text);

            var expectedText = @"
                This is a replacement. At this span.
            ".NormalizeCode();
            var expectedSpans = new[]
            {
                new TextSpan(4, 0),
                new TextSpan(5, 2),
                new TextSpan(25, 0),
                new TextSpan(31, 4)
            };
            var expectedChanges = new[]
            {
                new TextChange(new TextSpan(10, 11), "change"),
                new TextChange(new TextSpan(31, 0), "brand new ")
            };

            Assert.Equal(expectedText, annotatedText.Text);
            Assert.Equal(expectedSpans, annotatedText.Spans);
            Assert.Equal(expectedChanges, annotatedText.Changes);
        }

        [Fact]
        public static void AnnotatedText_DetectsMissingBrace_ForSpan()
        {
            var text = "Here { is a brace missing.";

            var exception = Assert.Throws<FormatException>(() => AnnotatedText.Parse(text));
            Assert.Equal("Missing '}' at position 26.", exception.Message);
        }

        [Fact]
        public static void AnnotatedText_DetectsMissingBrace_ForChange()
        {
            var text = "Here {is: a brace missing.";

            var exception = Assert.Throws<FormatException>(() => AnnotatedText.Parse(text));
            Assert.Equal("Missing '}' at position 26.", exception.Message);
        }

        [Fact]
        public static void AnnotatedText_DetectsColonOutsideOfChange()
        {
            var text = "This : is an error.";

            var exception = Assert.Throws<FormatException>(() => AnnotatedText.Parse(text));
            Assert.Equal("Character ':' is unexpected at position 5.", exception.Message);
        }
    }
}