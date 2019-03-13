#nullable enable

using System;

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