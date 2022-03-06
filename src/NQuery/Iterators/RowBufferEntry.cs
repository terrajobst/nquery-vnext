namespace NQuery.Iterators
{
    internal struct RowBufferEntry : IEquatable<RowBufferEntry>
    {
        public RowBufferEntry(RowBuffer rowBuffer, int index)
        {
            RowBuffer = rowBuffer;
            Index = index;
        }

        public RowBuffer RowBuffer { get; }

        public int Index { get; }

        public object GetValue()
        {
            return RowBuffer[Index];
        }

        public bool Equals(RowBufferEntry other)
        {
            return RowBuffer == other.RowBuffer &&
                   Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is RowBufferEntry && Equals((RowBufferEntry)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RowBuffer, Index);
        }

        public static bool operator ==(RowBufferEntry left, RowBufferEntry right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RowBufferEntry left, RowBufferEntry right)
        {
            return !left.Equals(right);
        }
    }
}