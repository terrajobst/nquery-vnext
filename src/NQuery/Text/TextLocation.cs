namespace NQuery.Text
{
    public struct TextLocation : IEquatable<TextLocation>
    {
        public TextLocation(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; }

        public int Column { get; }

        public bool Equals(TextLocation other)
        {
            return Line == other.Line && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            return obj is TextLocation other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Line, Column);
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
            return $"Ln {Line} Col {Column}";
        }
    }
}