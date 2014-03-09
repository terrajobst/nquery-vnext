using System;

namespace NQuery.Text
{
    public struct TextSpan : IEquatable<TextSpan>
    {
        private readonly int _start;
        private readonly int _length;

        public TextSpan(int start, int length)
        {
            _start = start;
            _length = length;
        }

        public static TextSpan FromBounds(int start, int end)
        {
            var length = end - start;
            return new TextSpan(start, length);
        }

        public int Start
        {
            get { return _start; }
        }

        public int End
        {
            get { return _start + _length; }
        }

        public int Length
        {
            get { return _length; }
        }

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
            return (maxStart < minEnd);
        }

        public bool IntersectsWith(TextSpan span)
        {
            return span.Start <= End && span.End >= Start;
        }

        public bool Equals(TextSpan other)
        {
            return _start == other._start &&
                   _length == other._length;
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
                return (_start*397) ^ _length;
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
            return string.Format("[{0},{1})", Start, End);
        }
    }
}