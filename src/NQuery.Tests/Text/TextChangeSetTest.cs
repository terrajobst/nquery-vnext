using System;

using NQuery.Text;

using Xunit;

namespace NQuery.Tests.Text
{
    public class TextChangeSetTest
    {
        [Fact]
        public void TextChangeSet_AddsInsertion()
        {
            var changeSet = new TextChangeSet();
            changeSet.InsertText(42, "abc");

            var change = Assert.Single(changeSet);
            Assert.Equal(new TextSpan(42, 0), change.Span);
            Assert.Equal("abc", change.NewText);
        }

        [Fact]
        public void TextChangeSet_AddsReplacement()
        {
            var changeSet = new TextChangeSet();
            changeSet.ReplaceText(new TextSpan(4, 2), "abc");

            var change = Assert.Single(changeSet);
            Assert.Equal(new TextSpan(4, 2), change.Span);
            Assert.Equal("abc", change.NewText);
        }

        [Fact]
        public void TextChangeSet_AddsDeletion()
        {
            var changeSet = new TextChangeSet();
            changeSet.DeleteText(new TextSpan(4, 2));

            var change = Assert.Single(changeSet);
            Assert.Equal(new TextSpan(4, 2), change.Span);
            Assert.Equal(string.Empty, change.NewText);
        }

        [Fact]
        public void TextChangeSet_ReturnsChangesInExecutionOrder()
        {
            var changeSet = new TextChangeSet();

            changeSet.ReplaceText(new TextSpan(10, 2), "xyz");
            changeSet.InsertText(1, "ab");
            changeSet.DeleteText(new TextSpan(4, 2));

            var expectedChanges = new[]
            {
                new TextChange(new TextSpan(10, 2), "xyz"),
                new TextChange(new TextSpan(1, 0), "ab"),
                new TextChange(new TextSpan(4, 2), string.Empty),
            };

            Assert.Equal(expectedChanges, changeSet);
        }

        [Fact]
        public void TextChangeSet_DisallowsIntersectingChanges()
        {
            var changeSet = new TextChangeSet();

            changeSet.DeleteText(new TextSpan(4, 2));

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                changeSet.InsertText(5, "abc");
            });

            Assert.Equal("Cannot apply change '[5,5) => {abc}' because it intersects with another pending change.", ex.Message);
        }

        [Theory]
        [InlineData("lorem *ipsum{:xyz}")]
        [InlineData("lorem ipsum{:xyz}*")]
        [InlineData("lorem *{ipsum:xyz}")]
        [InlineData("lorem {ipsum:xyz}*")]
        [InlineData("lorem *{ipsum:}")]
        [InlineData("lorem {ipsum:}*")]
        [InlineData("lorem {ip*sum:xy*z}")]
        [InlineData("lorem {ips*um:xyz*}")]
        [InlineData("lorem {ipsu*m:xyz*}")]
        [InlineData("lorem {ipsum*:xyz*}")]
        public void TextChangeSet_TranslatesPosition(string text)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var change = Assert.Single(annotatedText.Changes);
            Assert.Empty(annotatedText.Spans);

            var oldText = annotatedText.Text;
            var oldPosition = oldText.IndexOf("*");
            Assert.True(oldPosition >= 0);

            var changeSet = new TextChangeSet();
            changeSet.Add(change);

            var newText = SourceText.From(oldText).WithChanges(change).GetText();
            var newPosition = changeSet.TranslatePosition(oldPosition);
            Assert.Equal('*', newText[newPosition]);
        }
    }
}