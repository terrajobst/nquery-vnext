using System;

namespace NQuery.Text
{
    public struct TextLocation : IEquatable<TextLocation>
    {
        private readonly int _line;
        private readonly int _column;

        public TextLocation(int line, int column)
        {
            _line = line;
            _column = column;
        }

        public int Line
        {
            get { return _line; }
        }

        public int Column
        {
            get { return _column; }
        }

        public bool Equals(TextLocation other)
        {
            return _line == other._line && _column == other._column;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextLocation?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_line*397) ^ _column;
            }
        }

        public static bool operator ==(TextLocation left, TextLocation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextLocation left, TextLocation right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("Ln {0} Col {1}", Line, Column);
        }
    }
}