using System;

namespace NQuery.Language
{
    public struct TextSpan
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
    }
}