using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Text
{
    public struct TextSpan : IEquatable<TextSpan>
    {
        public TextSpan(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            Start = start;
            Length = length;
        }

        public static TextSpan FromBounds(int start, int end)
        {
            var length = end - start;
            return new TextSpan(start, length);
        }

        public int Start { get; }

        public int End
        {
            get { return Start + Length; }
        }

        public int Length { get; }

        public bool Contains(int position)
        {
            return Start <= position && position < End;
        }

        public bool ContainsOrTouches(int position)
        {
            return Contains(position) || position == End;
        }

        public bool Contains(TextSpan textSpan)
        {
            return Start <= textSpan.Start && textSpan.End <= End;
        }

        public bool OverlapsWith(TextSpan span)
        {
            var maxStart = Math.Max(Start, span.Start);
            var minEnd = Math.Min(End, span.End);
            return maxStart < minEnd;
        }

        public bool IntersectsWith(TextSpan span)
        {
            return span.Start <= End && span.End >= Start;
        }

        public TextSpan ApplyChanges(IEnumerable<TextChange> changes)
        {
            if (changes == null)
                throw new ArgumentNullException("changes");

            return changes.Aggregate(this, ApplyChange);
        }

        public TextSpan ApplyChange(TextChange textChange)
        {
            return ApplyChange(this, textChange);
        }

        private static TextSpan ApplyChange(TextSpan textSpan, TextChange textChange)
        {
            // NOTE: Other frameworks allow the consumer to specify whether changes
            //       at the edges should be merged with the span or not.
            //       Should we do the same?

            var delta = textChange.NewText.Length - textChange.Span.Length;

            if (!textSpan.IntersectsWith(textChange.Span))
            {
                // No overlap -- if the change happened after the given span,
                // we don't have to anything.

                if (textChange.Span.End >= textSpan.Start)
                    return textSpan;

                // Otherwise, we simply need to offset the start.

                return new TextSpan(textSpan.Start + delta, textSpan.Length);
            }

            var mergedStart = Math.Min(textSpan.Start, textChange.Span.Start);
            var mergedEnd = Math.Max(textSpan.End, textChange.Span.End);
            var mergedSpan = FromBounds(mergedStart, mergedEnd);
            var resultStart = mergedSpan.Start;
            var resultLength = mergedSpan.Length + delta;
            return new TextSpan(resultStart, resultLength);
        }

        public bool Equals(TextSpan other)
        {
            return Start == other.Start &&
                   Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextSpan?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start*397) ^ Length;
            }
        }

        public static bool operator ==(TextSpan left, TextSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextSpan left, TextSpan right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"[{Start},{End})";
        }
    }
}