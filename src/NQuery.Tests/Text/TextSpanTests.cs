using System;

using NQuery.Text;

using Xunit;

namespace NQuery.Tests.Text
{
    public sealed class TextSpanTests
    {
        [Fact]
        public void TextSpan_IntializeStartAndLength()
        {
            var textSpan = new TextSpan(1, 2);
            Assert.Equal(1, textSpan.Start);
            Assert.Equal(2, textSpan.Length);
            Assert.Equal(3, textSpan.End);
        }

        [Fact]
        public void TextSpan_IntializeStartAndEnd()
        {
            var textSpan = TextSpan.FromBounds(1, 3);
            Assert.Equal(1, textSpan.Start);
            Assert.Equal(2, textSpan.Length);
            Assert.Equal(3, textSpan.End);
        }

        [Fact]
        public void TextSpan_ContainsStart()
        {
            var textSpan = new TextSpan(1, 2);
            Assert.True(textSpan.Contains(textSpan.Start));
        }

        [Fact]
        public void TextSpan_ContainsItself()
        {
            var textSpan = new TextSpan(1, 2);
            Assert.True(textSpan.Contains(textSpan));
        }

        [Fact]
        public void TextSpan_DoesNotContainEnd()
        {
            var textSpan = new TextSpan(1, 2);
            Assert.False(textSpan.Contains(textSpan.End));
        }

        [Fact]
        public void TextSpan_ContainsOrTouchesStart()
        {
            var textSpan = new TextSpan(1, 2);
            Assert.True(textSpan.ContainsOrTouches(textSpan.Start));
        }

        [Fact]
        public void TextSpan_ContainsOrTouchesEnd()
        {
            var textSpan = new TextSpan(1, 2);
            Assert.True(textSpan.ContainsOrTouches(textSpan.End));
        }

        [Fact]
        public void TextSpan_OverlapsWithSelf()
        {
            var textSpan = new TextSpan(1, 2);
            Assert.True(textSpan.OverlapsWith(textSpan));
        }

        [Fact]
        public void TextSpan_OverlapsWhenOverhanging()
        {
            var textSpan1 = TextSpan.FromBounds(2, 5);
            var textSpan2 = TextSpan.FromBounds(3, 7);
            Assert.True(textSpan1.OverlapsWith(textSpan2));
            Assert.True(textSpan2.OverlapsWith(textSpan1));
        }

        [Fact]
        public void TextSpan_OverlapsWhenContained()
        {
            var textSpan1 = TextSpan.FromBounds(2, 8);
            var textSpan2 = TextSpan.FromBounds(3, 6);
            Assert.True(textSpan1.OverlapsWith(textSpan2));
            Assert.True(textSpan2.OverlapsWith(textSpan1));
        }

        [Fact]
        public void TextSpan_DoesNotOverlapAtEnd()
        {
            var textSpan1 = TextSpan.FromBounds(1, 2);
            var textSpan2 = TextSpan.FromBounds(2, 3);
            Assert.False(textSpan1.OverlapsWith(textSpan2));
            Assert.False(textSpan2.OverlapsWith(textSpan1));
        }

        [Fact]
        public void TextSpan_DoesNotOverlapWithEmpty()
        {
            var empty = new TextSpan(1, 0);
            Assert.False(empty.OverlapsWith(empty));
        }

        [Fact]
        public void TextSpan_IntersectsWithSelf()
        {
            var textSpan = new TextSpan(2, 4);
            Assert.True(textSpan.IntersectsWith(textSpan));
        }

        [Fact]
        public void TextSpan_IntersectsWhenOverhanging()
        {
            var textSpan1 = TextSpan.FromBounds(2, 5);
            var textSpan2 = TextSpan.FromBounds(3, 7);
            Assert.True(textSpan1.IntersectsWith(textSpan2));
            Assert.True(textSpan2.IntersectsWith(textSpan1));
        }

        [Fact]
        public void TextSpan_IntersectsWhenContained()
        {
            var textSpan1 = TextSpan.FromBounds(2, 8);
            var textSpan2 = TextSpan.FromBounds(3, 6);
            Assert.True(textSpan1.IntersectsWith(textSpan2));
            Assert.True(textSpan2.IntersectsWith(textSpan1));
        }

        [Fact]
        public void TextSpan_IntersectsAtEnd()
        {
            var textSpan1 = TextSpan.FromBounds(1, 2);
            var textSpan2 = TextSpan.FromBounds(2, 3);
            Assert.True(textSpan1.IntersectsWith(textSpan2));
            Assert.True(textSpan2.IntersectsWith(textSpan1));
        }

        [Fact]
        public void TextSpan_IntersectsWithEmpty()
        {
            var empty = new TextSpan(1, 0);
            Assert.True(empty.IntersectsWith(empty));
        }

        [Fact]
        public void TextSpan_EqualWithSelf()
        {
            var textSpan1 = new TextSpan(1, 2);
            var textSpan2 = textSpan1;
            Assert.Equal(textSpan1, textSpan2);
            Assert.True(textSpan1 == textSpan2);
        }

        [Fact]
        public void TextSpan_NotEqualWithOther()
        {
            var textSpan1 = new TextSpan(1, 2);
            var textSpan2 = new TextSpan(2, 3);
            Assert.NotEqual(textSpan1, textSpan2);
            Assert.True(textSpan1 != textSpan2);
        }

        [Fact]
        public void TextSpan_ToStringIsReadable()
        {
            var textSpan = TextSpan.FromBounds(1, 3);
            Assert.Equal("[1,3)", textSpan.ToString());
        }

        [Fact]
        public void TextSpan_ApplyChanges_Repositions_IfInsertionIsAfterBefore()
        {
            var originalSpan = TextSpan.FromBounds(10, 15);
            var change = TextChange.ForInsertion(5, "test");

            var expectedSpan = new TextSpan(originalSpan.Start + change.NewText.Length, originalSpan.Length);
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }

        [Fact]
        public void TextSpan_ApplyChanges_IsNoOp_IfInsertionIsAfter()
        {
            var originalSpan = TextSpan.FromBounds(10, 15);
            var change = TextChange.ForInsertion(20, "test");

            var expectedSpan = originalSpan;
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }

        [Fact]
        public void TextSpan_ApplyChanges_IsNoOp_IfReplacementIsContained()
        {
            var originalSpan = TextSpan.FromBounds(10, 15);
            var change = TextChange.ForReplacement(TextSpan.FromBounds(11, 14), "abc");

            var expectedSpan = originalSpan;
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }

        [Fact]
        public void TextSpan_ApplyChanges_IsNoOp_IfReplacingWithSame()
        {
            var originalSpan = TextSpan.FromBounds(5, 9);
            var change = TextChange.ForReplacement(TextSpan.FromBounds(5, 9), "test");

            var expectedSpan = originalSpan;
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }

        [Fact]
        public void TextSpan_ApplyChanges_IsMerged_IfInsertionAtStart()
        {
            var originalSpan = TextSpan.FromBounds(10, 15);
            var change = TextChange.ForInsertion(10, "test");

            var expectedSpan = TextSpan.FromBounds(10, 19);
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }

        [Fact]
        public void TextSpan_ApplyChanges_IsMerged_IfInsertionAtEnd()
        {
            var originalSpan = TextSpan.FromBounds(10, 15);
            var change = TextChange.ForInsertion(15, "test");

            var expectedSpan = TextSpan.FromBounds(10, 19);
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }

        [Fact]
        public void TextSpan_ApplyChanges_IsMerged_IfReplacementAtStart()
        {
            var originalSpan = TextSpan.FromBounds(10, 15);
            var change = TextChange.ForReplacement(new TextSpan(8, 4), "ab");

            var expectedSpan = TextSpan.FromBounds(8, 13);
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }

        [Fact]
        public void TextSpan_ApplyChanges_IsMerged_IfReplacementAtEnd()
        {
            var originalSpan = TextSpan.FromBounds(10, 15);
            var change = TextChange.ForReplacement(new TextSpan(13, 4), "ab");

            var expectedSpan = TextSpan.FromBounds(10, 15);
            var actualSpan = originalSpan.ApplyChange(change);
            Assert.Equal(expectedSpan, actualSpan);
        }
    }
}